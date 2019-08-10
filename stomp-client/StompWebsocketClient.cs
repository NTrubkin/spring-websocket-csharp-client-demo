using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using static NikitaTrubkin.StompClient.StompConnectionState;

namespace NikitaTrubkin.StompClient
{
    public class StompWebsocketClient : IStompClient
    {
        // todo: implement this
#pragma warning disable CS0067 // Событие "StompWebsocketClient.OnOpen" никогда не используется.
        public event EventHandler OnOpen;
#pragma warning restore CS0067 // Событие "StompWebsocketClient.OnOpen" никогда не используется.
#pragma warning disable CS0067 // Событие "StompWebsocketClient.OnClose" никогда не используется.
        public event EventHandler<CloseEventArgs> OnClose;
#pragma warning restore CS0067 // Событие "StompWebsocketClient.OnClose" никогда не используется.
#pragma warning disable CS0067 // Событие "StompWebsocketClient.OnError" никогда не используется.
        public event EventHandler<ErrorEventArgs> OnError;
#pragma warning restore CS0067 // Событие "StompWebsocketClient.OnError" никогда не используется.

        private readonly WebSocket socket;
        private readonly StompMessageSerializer stompSerializer = new StompMessageSerializer();

        private readonly IDictionary<string, Subscriber> subscribers = new Dictionary<string, Subscriber>();

        public StompConnectionState State { get; private set; } = Closed;

        public StompWebsocketClient(string url)
        {
            socket = new WebSocket(url);
        }

        public void Connect(IDictionary<string, string> headers)
        {
            if (State != Closed)
                throw new InvalidOperationException("The current state of the connection is not Closed.");

            socket.Connect();

            var connectMessage = new StompMessage(StompCommand.Connect, headers);
            socket.Send(stompSerializer.Serialize(connectMessage));
            // todo: check response

            socket.OnMessage += HandleMessage;
            State = Open;
        }

        public void Send(object body, string destination, IDictionary<string, string> headers)
        {
            if (State != Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            var jsonPayload = JsonConvert.SerializeObject(body);
            headers.Add("destination", destination);
            headers.Add("content-type", "application/json;charset=UTF-8");
            headers.Add("content-length", Encoding.UTF8.GetByteCount(jsonPayload).ToString());
            var connectMessage = new StompMessage(StompCommand.Send, jsonPayload, headers);
            socket.Send(stompSerializer.Serialize(connectMessage));
        }

        public void Subscribe<T>(string topic, IDictionary<string, string> headers, EventHandler<T> handler)
        {
            if (State != Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            headers.Add("destination", topic);
            headers.Add("id", "stub"); // todo: study and implement
            var subscribeMessage = new StompMessage(StompCommand.Subscribe, headers);
            socket.Send(stompSerializer.Serialize(subscribeMessage));
            // todo: check response
            // todo: implement advanced topic
            var sub = new Subscriber((sender, body) => handler(this, (T)body), typeof(T));
            subscribers.Add(topic, sub);
        }

        public void Dispose()
        {
            if (State != Open)
                throw new InvalidOperationException("The current state of the connection is not Open.");

            State = Closed;
            ((IDisposable)socket).Dispose();
            // todo: unsubscribe
        }

        private void HandleMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = stompSerializer.Deserialize(messageEventArgs.Data);
            var sub = subscribers[message.Headers["destination"]];
            var body = JsonConvert.DeserializeObject(message.Body, sub.BodyType);
            sub.Handler(sender, body);
        }
    }
}