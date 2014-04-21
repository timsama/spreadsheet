/*
 * SS_Server.cc
 *
 * This is a spreadsheet server responsible for handling changes from multiple clients editing
 * a single spreadsheet.  
 *
 */

#include "SS_Server.h"

// construct a spreadsheet object for a specific file
SS_Server::SS_Server(std::string fn)
  : ss(fn)
{
}

// destructor
SS_Server::~SS_Server()
{
}

// create a new thread to loop until a specific client closes a specific spreadsheet
void SS_Server::socket_loop_thread(Serv_Sock* sock)
{
   boost::thread workerThread(boost::bind(&SS_Server::socket_loop, this, boost::ref(sock)));
}

// create a new thread to loop until all users have closed a certian spreadsheet
void SS_Server::server_loop_thread()
{
   boost::thread workerThread(boost::bind(&SS_Server::server_loop, this));
}

// add a socket to a spreadsheet server so that it sends the socket updates of that spreadsheet
void SS_Server::add_sock(Serv_Sock* sock)
{
  // add the given socket to the set
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

  // Send the inital state of the spreadsheet to the newly joined socket
  send_message = MessageHandler::Update(ss.get_version(), ss.get_cells());
  SS_Server::broadcast(send_message, sock);

  while(run) {
    // wait for a message from the sock
    // receive
    message = sock->serv_recv();
    if(message.compare("")==0) {
      run = false;
      continue; // Add this to bypass all the following on disconnect
    }
    
    printf("%3d <- %s", sock->id, MessageHandler::readable(message));
    
    // create a message handler for the received message
    MessageHandler mh(message, sock);
    
    // if the message is an undo or enter type let the server_loop handle the return
    if ((mh.key.compare("UNDO")==0)||(mh.key.compare("ENTER")==0))
      {
	// lock the messages queue and the message handler to it
	{
	  boost::mutex::scoped_lock lock(guard);
	  messages.push(mh);
	}
      }
    // else determine the return message based on it
    else
      {
	// use message handler to format a message and broadcast
	if(mh.key.compare("RESYNC")==0)
	  {
	    send_message = MessageHandler::Sync(ss.get_version(), ss.get_cells());
	  }
	  else if (mh.key.compare("DISCONNECT")==0)
	    {
	      // the client has purposefully closed the spreadsheet
	      // disconnect the socket from the current ss_server
	      run = false;
	      continue;
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

  this->disconnect(sock);
  return;
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
}


// The server_loop processes messages in the messages queue as long as it is not empty
// It continues to check the queue unless the sockets set is empty
// It broadcasts response messages to all sockets in the sockets set
void SS_Server::server_loop()
{
  //printf("Inside server_loop the size of messages is %d for SS_Server %d.\n",messages.size(),this);
  // while the sockets set is not empty  
  while(true) //!sockets.empty())
    {
      //printf("Inside server_loop inside sockets.empty the size of messages is %d.\n",messages.size());
      // while the queue of message handlers is not empty
      while(!messages.empty())
	{
	  // pop the messge off of the queue
	  //printf("Inside server_loop.  The queue is not empty.\n");

	  MessageHandler message;

	  // lock the queue 
	  {
	    boost::mutex::scoped_lock lock(guard);
	    message = messages.front();
	    messages.pop();
	  }

	  int currentversion = ss.get_version();

	  if(message.version != boost::lexical_cast<std::string>(currentversion)) {
	    // Client -> SYNC with latest version
	    SS_Server::broadcast(MessageHandler::Sync(ss.get_version(), ss.get_cells()), message.socket);
	    continue;
	  } 

	  int newversion = 0;
	  std::string cell = "";
	  std::string contents = "";

	  if(message.key == "ENTER") {
	    newversion = ss.enter(message.cell, message.content);
	    cell = message.cell;
	    contents = message.content;
	  }

	  if(message.key == "UNDO") {
	    newversion = ss.undo(&cell, &contents);
	  }

	  if(newversion == -1) {
	    // Client -> Circular dependency
	    SS_Server::broadcast(MessageHandler::Error("Setting " + cell + contents + " results in a circular dependency."), message.socket);
	    continue;
	  }

	  if(newversion == 0) {
	    // Client -> Database error
	    SS_Server::broadcast(MessageHandler::Error("Unable to change the value for " + cell), message.socket);
	    continue;
	  }

	  // Everyone -> Update
	  SS_Server::broadcast(MessageHandler::Update(newversion, cell, contents));

	}
      // loop
      // sleep for 10 ms
      usleep(100000);
    }// end of while
  printf("Inside server loop no more sockets editting the spread\n");
}  

// send a message to every socket in the serv_sock list of sockets
void SS_Server::broadcast(std::string message)
{

  printf("\n%3dx-> %s", this->sockets.size(), MessageHandler::readable(message));

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

  printf("\n%3d -> %s", sock->id, MessageHandler::readable(message));

  sock->serv_send(message);
}
