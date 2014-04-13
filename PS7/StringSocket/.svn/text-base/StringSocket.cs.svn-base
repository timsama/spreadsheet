using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace CustomNetworking
{
    /// <summary> 
    /// A StringSocket is a wrapper around a Socket.  It provides methods that
    /// asynchronously read lines of text (strings terminated by newlines) and 
    /// write strings. (As opposed to Sockets, which read and write raw bytes.)  
    ///
    /// StringSockets are thread safe.  This means that two or more threads may
    /// invoke methods on a shared StringSocket without restriction.  The
    /// StringSocket takes care of the synchonization.
    /// 
    /// Each StringSocket contains a Socket object that is provided by the client.  
    /// A StringSocket will work properly only if the client refrains from calling
    /// the contained Socket's read and write methods.
    /// 
    /// If we have an open Socket s, we can create a StringSocket by doing
    /// 
    ///    StringSocket ss = new StringSocket(s, new UTF8Encoding());
    /// 
    /// We can write a string to the StringSocket by doing
    /// 
    ///    ss.BeginSend("Hello world", callback, payload);
    ///    
    /// where callback is a SendCallback (see below) and payload is an arbitrary object.
    /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
    /// successfully written the string to the underlying Socket, or failed in the 
    /// attempt, it invokes the callback.  The parameters to the callback are a
    /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
    /// the Exception that caused the send attempt to fail.
    /// 
    /// We can read a string from the StringSocket by doing
    /// 
    ///     ss.BeginReceive(callback, payload)
    ///     
    /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
    /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
    /// string of text terminated by a newline character from the underlying Socket, or
    /// failed in the attempt, it invokes the callback.  The parameters to the callback are
    /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
    /// string or the Exception will be non-null, but nor both.  If the string is non-null, 
    /// it is the requested string (with the newline removed).  If the Exception is non-null, 
    /// it is the Exception that caused the send attempt to fail.
    /// </summary>

    public class StringSocket
    {
        Socket wrappedSocket;

        // set the duration (in milliseconds) a thread should sleep between BeginReceive calls
        int SLEEP_DELAY = 50;

        // These delegates describe the callbacks that are used for sending and receiving strings.
        public delegate void SendCallback(Exception e, object payload);
        public delegate void ReceiveCallback(String s, Exception e, object payload);

        // these queues hold the send requests and receive requests
        private Queue<SendRequest> SendQueue;
        private Queue<ReceiveRequest> ReceiveQueue;
        
        // this queue holds unused strings
        private Queue<String> WholeStrings;

        // objects for locking
        private readonly Object SendLock;
        private readonly Object ReceiveLock;

        // our own buffer to append incoming buffers to
        private String socketBuffer;

        // the hashcode for this StringSocket
        private int HashCode;

        /// <summary>
        /// Creates a StringSocket from a regular Socket, which should already be connected.  
        /// The read and write methods of the regular Socket must not be called after the
        /// LineSocket is created.  Otherwise, the StringSocket will not behave properly.  
        /// The encoding to use to convert between raw bytes and strings is also provided.
        /// </summary>
        public StringSocket(Socket s, Encoding e)
        {
            wrappedSocket = s;
            HashCode = (int) DateTime.Now.ToFileTime();
            SendQueue = new Queue<SendRequest>();
            ReceiveQueue = new Queue<ReceiveRequest>();
            WholeStrings = new Queue<String>();
            SendLock = new Object();
            ReceiveLock = new Object();
        }

        /// <summary>
        /// Clears the request queues and closes the socket contained in this StringSocket
        /// </summary>
        public void CloseSocket()
        {
            lock (SendLock)
            {
                SendQueue.Clear();
                ReceiveQueue.Clear();

                if (wrappedSocket.Connected)
                {
                    wrappedSocket.Shutdown(SocketShutdown.Both);
                    wrappedSocket.Close();
                }
            }
        }

        /// <summary>
        /// Returns the HashCode for this StringSocket
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode;
        }

        /// <summary>
        /// We can write a string to a StringSocket ss by doing
        /// 
        ///    ss.BeginSend("Hello world", callback, payload);
        ///    
        /// where callback is a SendCallback (see below) and payload is an arbitrary object.
        /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
        /// successfully written the string to the underlying Socket, or failed in the 
        /// attempt, it invokes the callback.  The parameters to the callback are a
        /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
        /// the Exception that caused the send attempt to fail. 
        /// 
        /// This method is non-blocking.  This means that it does not wait until the string
        /// has been sent before returning.  Instead, it arranges for the string to be sent
        /// and then returns.  When the send is completed (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginSend
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginSend must take care of synchronization instead.  On a given StringSocket, each
        /// string arriving via a BeginSend method call must be sent (in its entirety) before
        /// a later arriving string can be sent.
        /// </summary>
        public void BeginSend(String s, SendCallback callback, object payload)
        {
            lock (SendLock)
            {
                // initialize buffer attributes
                byte[] buffer = UTF8Encoding.UTF8.GetBytes(s);
                int size = buffer.Length;
                int offset = 0;

                // set up the delegate payload association
                SendQueue.Enqueue(new SendRequest(s, callback, payload));
                
                // if the item we just added is the only one in the queue, try to send
                if (SendQueue.Count == 1)
                {
                    try
                    {
                        wrappedSocket.BeginSend(buffer, offset, size, SocketFlags.None, InterimSendCallback, payload);
                    }
                    catch (SocketException)
                    {
                        SendQueue.Dequeue();
                    }
                }
            }
        }

        /// <summary>
        /// Called by the socket when a BeginSend has ended. Calls the user's callback when the Send has completed.
        /// </summary>
        /// <param name="result"></param>
        private void InterimSendCallback(IAsyncResult result)
        {
            // set the offset to the number of bytes sent last time
            int offset = wrappedSocket.EndSend(result);

            lock (SendLock)
            {
                // we're done sending, dequeue the request and call callback
                SendRequest s = SendQueue.Dequeue();
                s.CallCallback();

                // if the queue of SendRequests has items remaining, get the next one and execute it
                if (SendQueue.Count != 0)
                {
                    // get the next SendRequest's information
                    s = SendQueue.Peek();

                    // set the new buffer to be the next message to send
                    byte[] buffer = UTF8Encoding.UTF8.GetBytes(s.Message);

                    // try to send the entire buffer
                    wrappedSocket.BeginSend(buffer, 0, s.Message.Length, SocketFlags.None, InterimSendCallback, s.GetPayload());
                }
            }
        }

        /// <summary>
        /// 
        /// <para>
        /// We can read a string from the StringSocket by doing
        /// </para>
        /// 
        /// <para>
        ///     ss.BeginReceive(callback, payload)
        /// </para>
        /// 
        /// <para>
        /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
        /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
        /// string of text terminated by a newline character from the underlying Socket, or
        /// failed in the attempt, it invokes the callback.  The parameters to the callback are
        /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
        /// string or the Exception will be non-null, but nor both.  If the string is non-null, 
        /// it is the requested string (with the newline removed).  If the Exception is non-null, 
        /// it is the Exception that caused the send attempt to fail.
        /// </para>
        /// 
        /// <para>
        /// This method is non-blocking.  This means that it does not wait until a line of text
        /// has been received before returning.  Instead, it arranges for a line to be received
        /// and then returns.  When the line is actually received (at some time in the future), the
        /// callback is called on another thread.
        /// </para>
        /// 
        /// <para>
        /// This method is thread safe.  This means that multiple threads can call BeginReceive
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginReceive must take care of synchronization instead.  On a given StringSocket, each
        /// arriving line of text must be passed to callbacks in the order in which the corresponding
        /// BeginReceive call arrived.
        /// </para>
        /// 
        /// <para>
        /// Note that it is possible for there to be incoming bytes arriving at the underlying Socket
        /// even when there are no pending callbacks.  StringSocket implementations should refrain
        /// from buffering an unbounded number of incoming bytes beyond what is required to service
        /// the pending callbacks.        
        /// </para>
        /// 
        /// <param name="callback"> The function to call upon receiving the data</param>
        /// <param name="payload"> 
        /// The payload is "remembered" so that when the callback is invoked, it can be associated
        /// with a specific Begin Receiver....
        /// </param>  
        /// 
        /// <example>
        ///   Here is how you might use this code:
        ///   <code>
        ///                    client = new TcpClient("localhost", port);
        ///                    Socket       clientSocket = client.Client;
        ///                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());
        ///                    receiveSocket.BeginReceive(CompletedReceive1, 1);
        /// 
        ///   </code>
        /// </example>
        /// </summary>
        /// 
        /// 

        public void BeginReceive(ReceiveCallback callback, object payload)
        {
            // set up a 1kB buffer for incoming data
            byte[] buffer = new byte[1024];

            // set up a ReceiveRequest
            ReceiveRequest r = new ReceiveRequest(callback, payload);

            lock (ReceiveLock)
            {
                // set up the delegate and callback
                ReceiveQueue.Enqueue(r);

                // if this is the only ReceiveRequest on the queue, activate it
                if (ReceiveQueue.Count == 1)
                {
                    // if there are whole strings waiting to be used, use them
                    if (WholeStrings.Count > 0)
                    {
                        // process this request
                        ReceiveQueue.Dequeue();
                        r.Append(WholeStrings.Dequeue());
                        r.CallCallback();

                        // if there are more ReceiveRequests, process the next one
                        if (ReceiveQueue.Count > 0)
                        {
                            // begin receiving the next request
                            Thread.Sleep(SLEEP_DELAY);
                            wrappedSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, InterimReceiveCallback, buffer);
                        }
                    }
                    else
                    {
                        try
                        {
                            // tell the socket to start listening and adding data to this buffer
                            wrappedSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, InterimReceiveCallback, buffer);
                        }
                        catch (SocketException)
                        {
                            ReceiveQueue.Dequeue();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called by the socket when a BeginReceive has ended. Calls the user's callback when the Receive has completed.
        /// </summary>
        /// <param name="result"></param>
        private void InterimReceiveCallback(IAsyncResult result)
        {
            byte[] buffer = (byte[])(result.AsyncState);
            String message = UTF8Encoding.UTF8.GetString(buffer);

            lock (ReceiveLock)
            {
                // if the first character is a null character, then the sender disconnected
                if (message[0] == '\0' && (ReceiveQueue.Count > 0))
                {
                    ReceiveRequest r = ReceiveQueue.Peek();
                    r.ThrownException = new DisconnectedException();
                }

                // truncate message at first null character if there is one
                if (message.Contains('\0'))
                {
                    message = message.Substring(0, message.IndexOf('\0'));
                }

                // verify that the socket is still open before attempting to use it
                if ((wrappedSocket != null) && wrappedSocket.Connected)
                {
                    try
                    {
                        int bytes = wrappedSocket.EndReceive(result);

                        // add message to socket buffer
                        socketBuffer = socketBuffer + message;

                        // if no bytes were received, the connection was closed
                        if (bytes != 0)
                        {
                            // if the buffer has a newline, it's the end of this message
                            if (socketBuffer.Contains('\n') && (ReceiveQueue.Count > 0))
                            {
                                // if there are still unused characters on the socketBuffer, add complete strings
                                // to WholeStrings, and continue until there are no more newlines
                                while (socketBuffer.Contains('\n'))
                                {
                                    WholeStrings.Enqueue(socketBuffer.Substring(0, socketBuffer.IndexOf('\n')));
                                    socketBuffer = socketBuffer.Substring(socketBuffer.IndexOf('\n') + 1);
                                }

                                // match up whole strings with receive requests until one runs out
                                while ((WholeStrings.Count > 0) && (ReceiveQueue.Count > 0))
                                {
                                    // get this request off the stack, and add the last of its data to it
                                    ReceiveRequest r = ReceiveQueue.Dequeue();
                                    r.Append(WholeStrings.Dequeue());

                                    // wait for the sleep delay value, then call the callback of r
                                    Thread.Sleep(SLEEP_DELAY);
                                    r.CallCallback();
                                }

                                // if there are more ReceiveRequests, process the next one
                                if (ReceiveQueue.Count > 0)
                                {
                                    // begin receiving the next request
                                    Thread.Sleep(SLEEP_DELAY);
                                    wrappedSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, InterimReceiveCallback, buffer);
                                }
                            }
                            else
                            {
                                // the socketBuffer didn't have a newline, so begin receiving the next data set
                                Thread.Sleep(SLEEP_DELAY);
                                wrappedSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, InterimReceiveCallback, buffer);
                            }
                        }
                    }
                    catch (SocketException e) {
                        if (ReceiveQueue.Count > 0)
                        {
                            ReceiveRequest r = ReceiveQueue.Dequeue();
                            r.ThrownException = e;
                            r.CallCallback();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents an associated SendCallback and payload.
    /// </summary>
    public class SendRequest
    {
        //store the given callback and payload
        StringSocket.SendCallback callback;
        object payload;
        public readonly String Message;

        /// <summary>
        /// Create a SendRequest with the given callback and payload.
        /// </summary>
        /// <param name="_callback"></param>
        /// <param name="_payload"></param>
        public SendRequest(String _message, StringSocket.SendCallback _callback, object _payload)
        {
            callback = _callback;
            payload = _payload;
            Message = _message;
        }

        /// <summary>
        /// Call the callback associated with the payload.
        /// </summary>
        public void CallCallback()
        {
            // call this on a separate thread
            ThreadPool.QueueUserWorkItem(x => callback(null, payload));
        }

        /// <summary>
        /// Get the payload of this request
        /// </summary>
        /// <returns></returns>
        public Object GetPayload()
        {
            return payload;
        }
    }

    /// <summary>
    /// Represents an associated ReceiveCallback, payload and message.
    /// </summary>
    public class ReceiveRequest
    {
        //these store the given callback, payload, and message
        StringSocket.ReceiveCallback callback;
        object payload;
        String message;
        public Exception ThrownException;
        
        /// <summary>
        /// Creates a ReceiveRequest with the given callback and payload.
        /// </summary>
        /// <param name="_callback"></param>
        /// <param name="_payload"></param>
        public ReceiveRequest(StringSocket.ReceiveCallback _callback, object _payload)
        {
            callback = _callback;
            payload = _payload;
            message = "";
            ThrownException = null;
        }

        /// <summary>
        /// Calls the given callback based on the message and payload.
        /// </summary>
        public void CallCallback()
        {
            // call this on a separate thread
            ThreadPool.QueueUserWorkItem(x => callback(message, ThrownException, payload));
        }

        /// <summary>
        /// Appends the given string to the rest of the message.
        /// </summary>
        /// <param name="s"></param>
        public void Append(String s)
        {
            message = message + s;
        }
    }

    /// <summary>
    /// Represents a dropped connection
    /// </summary>
    public class DisconnectedException : Exception
    { }
 }