/*
 *SS_Server.cc
 */

#include "SS_Server.h"

SS_Server::SS_Server()
{
 
}

SS_Server::SS_Server(std::string fn, Serv_Sock sock)
{
  serv_sock = sock;
  socket = sock.id;
}

SS_Server::~SS_Server()
{
}

// Messages are passed from the Server to the SS_Server using this function
// The socket loop listens for messages from a specific socket and adds them
// them to the messages queue if they are an enter or undo command
// ELSE it immediately broadcasts the response to the given socket 
void SS_Server::socket_loop(Serv_Sock sock)
{
  int socket = sock.id;
  std::string message;
  std::string send_message;

  // add the given socket to the set
  sockets.insert(&serv_sock);

  while(1)
    {
      // wait for a message from the sock
      // receive
      message = sock.serv_recv();
      printf("%d: ", socket); 
      std::cout << "Here is the message: " << message << std::endl;
      
      // create a message handler for the received message
      MessageHandler mh(message, socket);
    
      // if the message is an undo or enter type let the server_loop handle the return mess
      if ((mh.key.compare("UNDO")==0)||(mh.key.compare("ENTER")==0))
	{
	  // lock the messages queue and the message handler to it
	  messages.push(mh);
	}
      // else determine the return message based on it
      else
	{
	  // use message handler to format a message and broadcast
	  if(mh.key.compare("SYNC")==0)
	    {
	      //send_message = mh.Sync(mh.version,...);
	    }
	  else if (mh.key.compare("SAVED")==0)
	    {
	    }
	  else if (mh.key.compare("UPDATE")==0)
	    {
	    }
	  else
	    {
	      // invalid message
	    }
	  // broadcast the return message to the provided sock
	  broadcast(send_message, socket);
	}
      
      // loop
    }// end of while
}



// The server_loop processes messages in the messages queue as long as it is not empty
// It continues to check the queue unless the sockets set is empty
// It broadcasts response messages to all sockets in the sockets set
void SS_Server::server_loop()
{
  int n;
  std::string return_message;

  // while the sockets set is not empty  
  while(!sockets.empty())
    {
      // while the queue of message handlers is not empty
      while(!messages.empty())
	{
	  // pop the messge off of the queue
	  MessageHandler new_mh = messages.front();
	  messages.pop();
	}
      // loop
    }// end of while

}  

// send a message to every socket in the serv_sock list of sockets
void SS_Server::broadcast(std::string message)
{
  // iterate through the open spreads map and determine if the given spread is in it
  for (std::set<Serv_Sock*>::iterator it = this->sockets.begin(); it != this->sockets.end(); ++it)
    {
      // send a message to the current socket
      (*it)->serv_send(message);
    }
}

// send a message to a specific socket 
void SS_Server::broadcast(std::string message, Serv_Sock sock)
{
  sock.serv_send(message);
}



