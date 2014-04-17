//StringSocket.h

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <sys/wait.h>
#include <signal.h>

#define PORT "3490"  // the port users will be connecting to

#define BACKLOG 10     // how many pending connections queue will hold

class StringSocket{
 
 private:

  // these queues hold the send requests and receive requests
  Queue<SendRequest> SendQueue;
  Queue<ReceiveRequest> ReceiveQueue;
        
  // this queue holds unused strings
  Queue<String> WholeStrings;

  // objects for locking
  readonly Object SendLock;
  readonly Object ReceiveLock;

  // our own buffer to append incoming buffers to
  string socketBuffer;

  // the hashcode for this StringSocket
  int HashCode;
        


 public:
  Socket wrappedSocket;

  // set the duration (in milliseconds) a thread should sleep between BeginReceive calls
  int SLEEP_DELAY = 50;

  // These delegates describe the callbacks that are used for sending and receiving strings.
  void SendCallback(Exception e, object payload);
  void ReceiveCallback(String s, Exception e, object payload);

};
