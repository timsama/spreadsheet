/*
 *SS_Server.cc
 */

#include "SS_Server.h"

SS_Server::SS_Server(std::string fn)
  : ss(fn)
{
}

SS_Server::~SS_Server()
{
}

void SS_Server::add_sock(Serv_Sock* sock)
{
  // add the given socket to the set
  printf("A socket was added to sockets.\n");
  sockets.insert(sock);
}

// Messages are passed from the Server to the SS_Server using this function
// The socket loop listens for messages from a specific socket and adds them
// them to the messages queue if they are an enter or undo command
// ELSE it immediately broadcasts the response to the given socket 
void SS_Server::socket_loop(Serv_Sock* sock)
{
  std::string message;
  std::string send_message;
  bool run = true;
  int count = 0;
  while(run)
  {
      // wait for a message from the sock
      // receive
      message = sock->serv_recv();
      if(message.compare("")==0)
	run = false;

      std::cout << "Here is the message inside of socket_loop: " << message << std::endl;
      
      // create a message handler for the received message
      MessageHandler mh(message, sock);
    
      // if the message is an undo or enter type let the server_loop handle the return mess
      if ((mh.key.compare("UNDO")==0)||(mh.key.compare("ENTER")==0))
	{
	  std::cout << "Pushing a " << mh.key << " command on to the queue.\n";
	  // lock the messages queue and the message handler to it
	  messages.push(mh);
	  printf("The queue is size %d inside of socket loop.\n",messages.size());
	}
      // else determine the return message based on it
      else
	{
	  // use message handler to format a message and broadcast
	  if(mh.key.compare("RESYNC")==0)
	    {
	      std::map<std::string,std::string> fakemap;
	      fakemap.insert(std::pair<std::string,std::string>("A2","goodbye"));
	      send_message = MessageHandler::Sync(2,fakemap);
	    }
	  else if (mh.key.compare("DISCONNECT")==0)
	    {
	      printf("Received DISCONNECT.\n");
	      // the client has purposefully closed the spreadsheet
	      // disconnect the socket from the current ss_server
	      run = false;
	      this->disconnect(sock);
	      return;
	    }
	  else if (mh.key.compare("SAVE")==0)
	    {
	      // do something with the version number
	      // check if the version number is correct
	      // if it is incorrect send sync instead of saving
	      send_message = MessageHandler::Saved();
	    }
	  else
	    {
	      // invalid message
	    }
	  // broadcast the return message to the provided sock
	  broadcast(send_message, sock);
	}
      // loop
      }// end of while
}

// remove the given socket from the sockets set and close it
void SS_Server::disconnect(Serv_Sock* sock)
{
 shutdown(sock->id, 2);
  // close the socket
  close(sock->id);
  // delete the serv_sock
  delete sock;
  // remove the pointer from the set
  sockets.erase(sock);

  printf("Inside disconnect function\n");
 
}


// The server_loop processes messages in the messages queue as long as it is not empty
// It continues to check the queue unless the sockets set is empty
// It broadcasts response messages to all sockets in the sockets set
void SS_Server::server_loop()
{
  printf("Inside server_loop the size of messages is %d.\n",messages.size());
  // while the sockets set is not empty  
  while(!sockets.empty())
    {
      printf("Inside server_loop inside sockets.empty the size of messages is %d.\n",messages.size());
      // while the queue of message handlers is not empty
      while(!messages.empty())
	{
	  // pop the messge off of the queue
	  printf("Inside server_loop.  The queue is not empty.\n");
	  MessageHandler new_mh = messages.front();
	  messages.pop();
	}
      // loop
      // sleep for 10 ms
      usleep(10000);
    }// end of while
  printf("Inside server loop no more sockets editting the spread\n");
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
void SS_Server::broadcast(std::string message, Serv_Sock* sock)
{
  sock->serv_send(message);
}



