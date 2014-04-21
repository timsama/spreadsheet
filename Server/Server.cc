/*
** Server.cc -- a stream socket server demo
*/

#include "Server.h"

// constructor
Server::Server()
{
}

// destructor
Server::~Server()
{
}

// handle the client on a new thread
void Server::handle_client_thread(Serv_Sock* serv_sock)
{
  boost::thread workerThread(boost::bind(&Server::handle_client, this, boost::ref(serv_sock)));
}

// call all of the necessary functions to handle the client and pass it to the SS_Server
void Server::handle_client(Serv_Sock* serv_sock)
{
  wait_authorize(serv_sock);
  std::string filename = wait_open_create(serv_sock);
  open_spreadsheet(serv_sock, filename);
}

// open a spreadsheet
void Server::open_spreadsheet(Serv_Sock* serv_sock, std::string filename)
{
  bool justcreated = false;

  // iterate through the open spreads map and determine if the given spread is in it
  std::map<std::string, SS_Server*>::iterator it;

  {
    // Lock while checking and adding spreadsheets to the map
    //boost::mutex::scoped_lock lock(guard2);
    it = this->open_spreads.find(filename);
    if(it == this->open_spreads.end())
      {
	justcreated = true;
	SS_Server* new_ss_s;
	new_ss_s = new SS_Server(filename);
	std::pair<std::string, SS_Server*> spread(filename, new_ss_s);
	this->open_spreads.insert(spread);
      }
  }

  // Start the server loop if we just created the SS_Server
  if(justcreated) {
      it = this->open_spreads.find(filename);
      it->second->server_loop_thread();
    }

  // Add the socket to the spreadsheet
  it->second->add_sock(serv_sock);

  // Start socket loop on a new thread
  it->second->socket_loop_thread(serv_sock);
}

// handle a client until a valid password is received
void Server::wait_authorize(Serv_Sock* serv_sock)
{
  std::string message;
  std::string send_message;
  bool run = true;

  // wait to receive a valid password message
  while(run)
    {
      message = serv_sock->serv_recv();
      if(message.compare("")==0)
	run = false;
      
      printf("%d <- %s", serv_sock->id, MessageHandler::readable(message)); 
       
      //for the password
      // create a message handler for the received message
      MessageHandler mh(message, serv_sock);
    
      if ((mh.key.compare("PASSWORD")==0))
	 {      
	   // if the password message was received and valid - exit while
	   if(mh.password.compare("warrior")==0)
	     {
	       break;
	     }
	   else
	     {
	       // send invalid
	       send_message = mh.Invalid();
	       serv_sock->serv_send(send_message);
	     }
	 }
      else if ((mh.key.compare("DISCONNECT")==0))
	{
	  close(serv_sock->id);
	}
       // if the message is not password send an error and loop
       else
	 { 
	   // send
	   send_message = mh.Error("Not a password command.");
	   serv_sock->serv_send(send_message);
	   printf("%d -> %s",serv_sock->id, MessageHandler::readable(send_message));
	 }
    }// end of while waiting for the password command

  // a valid password was received send filelist
  // use message handler to compose filelist from returnlist
  send_message = MessageHandler::Filelist(file_return());
  serv_sock->serv_send(send_message);
  printf("%d -> %s", serv_sock->id, MessageHandler::readable(send_message));
}

// return a list of files that exist in a specific folder
std::list<std::string> Server::file_return()
{
  std::list<std::string> mylist;
  /*
   * http://stackoverflow.com/questions/612097/how-can-i-get-a-list-of-files-in-a-directory-using-c-or-c
   * This small snip of code the second option show on this webpage.
   */
  DIR *dpdf;
  struct dirent *epdf;

  dpdf = opendir("./spreadsheets");
  if (dpdf != NULL)
    {
      while (epdf = readdir(dpdf))
	{
	  std::string str = epdf->d_name;
	  if(str.compare(".")!=0&&str.compare("..")!=0)
	    mylist.push_back(str);
	}
    }

  return mylist;
}

// handle a client until a valid open or create command is received
std::string Server::wait_open_create(Serv_Sock* serv_sock)
{
  std::string filename;
  std::string message;
  std::string send_message;
  bool run = true;

  // wait to receive an open message
   while(run)
    {
      // call receive to get the open message
      // receive
      message = serv_sock->serv_recv();
      if(message.compare("")==0)
	run = false;
      
      printf("%d <- %s", serv_sock->id, MessageHandler::readable(message));

      // if the open message was received exit while
      // create a message handler for the received message
      MessageHandler mh(message, serv_sock);


      // for the opening of a file

      bool open = mh.key.compare("OPEN")==0;
      bool create = mh.key.compare("CREATE")==0;

      
      if(open||create)
	{

	  bool found = false;
	  std::list<std::string> filelist = file_return();
	  // iterate through the list comparing each filename to the provided
	  for(std::list<std::string>::iterator it = filelist.begin(); it != filelist.end(); it++)
	    { 
	      if(mh.name.compare(*it)==0)
		found = true;
	    }
	  
	  if((found && open) || (!found && create))
	    {
	      // no error in message send spreadsheet
	      filename = mh.name;
	      break;
	    }
	  else if (open) 
	    {
	      // send error message for trying to open a non-existing file
	      send_message = mh.Error("The requested file did not exist.");
	      serv_sock->serv_send(send_message);
	    }
	  else
	    {
	      // send error message for trying to create an existing file
	      send_message = mh.Error("A spreadsheet already exists with the requested name.");
	      serv_sock->serv_send(send_message);
	    }
	}
       else if ((mh.key.compare("DISCONNECT")==0))
	{
	  close(serv_sock->id);
	}
      else
	{ 
	  // send
	  send_message = mh.Error("Not an open command.");
	  serv_sock->serv_send(send_message);
	  printf("%d -> %s", serv_sock->id, MessageHandler::readable(send_message));
	}
    } // end of while waiting for the open command

  // return the filename of the spreadsheet to be opened an edited
  return filename;

}
