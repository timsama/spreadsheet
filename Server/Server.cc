/*
** Server.cc -- a stream socket server demo
*/

#include "Server.h"

// constructor
Server::Server(Serv_Sock* sock)
  : serv_sock(sock)
{
  // NOT PERMANENT JUST DEBUGGING
  filelist.push_back("ss1");
  filelist.push_back("ss2");
  filelist.push_back("ss3");
  delimiter = "\e";
  endline = "\n";
}

// destructor
Server::~Server()
{
}

// call all of the necessary functions to handle the client and pass it to the SS_Server
void Server::handle_client()
{
  wait_authorize();
  std::string filename = wait_open_create();
  open_spreadsheet(filename);
}

void Server::open_spreadsheet(std::string filename)
{
  SS_Server* ss_s;
  // iterate through the open spreads map and determine if the given spread is in it
  std::map<std::string, SS_Server>::iterator it;
  it = this->open_spreads.find(filename);
  if(it == open_spreads.end())
    {
      // Create new
      SS_Server new_ss_s(filename);
      std::pair<std::string, SS_Server> spread(filename, new_ss_s);
      open_spreads.insert(spread);
      
      it = this->open_spreads.find(filename);
      it->second.add_sock(serv_sock);
      //Fork
      int pid = fork();
      if (pid < 0)
	{
	  perror("ERROR on fork");
	  exit(1);
	}
      if (pid == 0)  
	{
	  // child process
	  it->second.server_loop();
	  exit(0);
	}
    }
  else
    {
      // Use existing
      it->second.add_sock(serv_sock);
    }

  printf("Called socket_loop.\n");
  it->second.socket_loop(serv_sock);
}


void Server::wait_authorize()
{
  std::string message;
  std::string send_message;

  // wait to receive a valid password message
  while(1)
    {
      printf("%d: Waiting for password command...\n",serv_sock->id);
      
      message = serv_sock->serv_recv();
      printf("%d: ", serv_sock->id); 
      std::cout << "Here is the message: " << message << std::endl;
       
      // if the password message was received and valid exit while
      //std::string message(buffer);
       
      //for the password
      // create a message handler for the received message
      MessageHandler mh(message, serv_sock);
    
      // if the message is an undo or enter type let the server_loop handle the return mess
      if ((mh.key.compare("PASSWORD")==0))
	 {      
	   printf("%d: The PASSWORD command was received. ",serv_sock->id);

	   if(mh.password.compare("warrior")==0)
	     {
	       break;
	     }
	   else
	     {
	       // send invalid
	       send_message = mh.Invalid();
	       serv_sock->serv_send(send_message);
	       printf("The password was invalid and a message was sent.\n");
	     }
	 }
       
       // if the message is not password send an error and loop
       else
	 { 
	   // send
	   send_message = mh.Error("");
	   serv_sock->serv_send(send_message);
	   printf("%d: Not a password command.  An error message was sent.\n",serv_sock->id);
	 }
    }// end of while waiting for the password command

  // a valid password was received send filelist
  send_message = "FILELIST"+delimiter+"ss1.ss"+delimiter+"ss2.ss\n";
  serv_sock->serv_send(send_message);
  printf("The received password was valid.  The filelist was sent.\n");  
}


std::string Server::wait_open_create()
{
  std::string filename;
  std::string message;

  // wait to receive an open message
   while(1)
    {
      printf("%d: Waiting for open command...\n",serv_sock->id);

      // call receive to get the open message
      // receive
      message = serv_sock->serv_recv();

      printf("%d: Here is the message: %s\n", serv_sock->id, message.c_str());

      // if the open message was received exit while
      // create a message handler for the received message
      MessageHandler mh(message, serv_sock);


      // for the opening of a file
      std::string send_message;

      bool open = mh.key.compare("OPEN")==0;
      bool create = mh.key.compare("CREATE")==0;

      
      if(open||create)
	{
	  printf("%d: An open or receive message was received. ",serv_sock->id);
	  bool found = false;
	  for(int i = 0; i < filelist.size();i++)
	    {
	      if(filelist[i].compare(mh.content)==0)
		found = true;           
	    }

	  if(((found == true)&&open)||((found==false)&&create))
	    {
	      // no error in message send spreadsheet
	      printf("The message was valid. ");
	      break;
	    }
	  else if (open) 
	    {
	      // send error message for trying to open a non-existing file
	      send_message = mh.Error("The requested file did not exist.");
	      serv_sock->serv_send(send_message);
	      printf("The requested file did not exist. An error was sent.\n "); 
	    }
	  else
	    {
	      // send error message for trying to create an existing file
	      send_message = mh.Error("A spreadsheet already exists with the requested name.");
	      serv_sock->serv_send(send_message);
	      printf("A spreadsheet already exists with the requested name. An error was sent\n"); 
	    }
	  filename = mh.content;
	}
      else
	{ 
	  // send
	  send_message = mh.Error("Not an open command.");
	  serv_sock->serv_send(send_message);
	  printf("%d: Not an open command.  An error message was sent.\n",serv_sock->id);
	}
    }// end of while waiting for the open command

  // return the filename of the spreadsheet to be opened an edited
  return filename;

}
