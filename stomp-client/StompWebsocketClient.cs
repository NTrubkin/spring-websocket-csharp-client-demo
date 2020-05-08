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

        public const string CONNECTION_STATE = "The current state of the connection is not Closed.";
        // todo: implement this
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler OnOpen;
        public event EventHandler<CloseEventArgs> OnClose;

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
            socket.OnError += OnError;
            socket.OnClose += OnClose;
            socket.OnOpen += OnOpen;
            State = Open;
        }

        public void Send(object body, string destination, IDictionary<string, string> headers)
        {
            if (State != Open)
                throw new InvalidOperationException(CONNECTION_STATE);

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
                throw new InvalidOperationException(CONNECTION_STATE);

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (State != Open)
                throw new InvalidOperationException(CONNECTION_STATE);
            socket.Close();
            State = Closed;
            ((IDisposable)socket).Dispose();
        }

        private void HandleMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = stompSerializer.Deserialize(messageEventArgs.Data);

            if (message.Command == StompCommand.Message)
            {
                var key = message.Headers["destination"];

                if (subscribers.ContainsKey(key))
                {
                    var sub = subscribers[key];
                    if (sub.BodyType == typeof(string))
                    {
                        sub.Handler(sender, message.Body);
                    }
                    else
                    {
                        var body = JsonConvert.DeserializeObject(message.Body, sub.BodyType);
                        sub.Handler(sender, body);
                    }
                }
            }
        }
    }
}