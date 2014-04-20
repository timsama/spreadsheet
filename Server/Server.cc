/*
** Server.cc -- a stream socket server demo
*/

#include "Server.h"

// constructor
Server::Server(Serv_Sock* sock)
  : serv_sock(sock)
{
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
	  printf("Server_loop was started.\n");
	  it->second.server_loop();
	  printf("The pointer to the Serv_Sock is %h.", it);
	  exit(0);
	}
    }
  else
    {
      // Use existing
      it->second.add_sock(serv_sock);
    }

  it->second.socket_loop(serv_sock);
  printf("Finished socket loop\n");
}


void Server::wait_authorize()
{
  std::string message;
  std::string send_message;
  bool run = true;

  // wait to receive a valid password message
  while(run)
    {
      printf("%d: Waiting for password command...\n",serv_sock->id);
      
      message = serv_sock->serv_recv();
      if(message.compare("")==0)
	run = false;
      
      printf("%d: ", serv_sock->id); 
      std::cout << "Here is the message: " << message << std::endl;

       
      //for the password
      // create a message handler for the received message
      MessageHandler mh(message, serv_sock);
    
      if ((mh.key.compare("PASSWORD")==0))
	 {      
	   printf("%d: The PASSWORD command was received. ",serv_sock->id);
	   std::cout << "The PASSWORD received was " <<  mh.password << ".";
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
	       printf("The password was invalid and a message was sent.\n");
	     }
	 }
      else if ((mh.key.compare("DISCONNECT")==0))
	{
	  printf("A DISCONNECT command was received close the socket.\n");
	  close(serv_sock->id);
	}
       // if the message is not password send an error and loop
       else
	 { 
	   // send
	   send_message = mh.Error("Not a password command.");
	   serv_sock->serv_send(send_message);
	   printf("%d: Not a password command.  An error message was sent.\n",serv_sock->id);
	 }
    }// end of while waiting for the password command

  // a valid password was received send filelist
  // use message handler to compose filelist from returnlist
  send_message = MessageHandler::Filelist(file_return());
  serv_sock->serv_send(send_message);
  printf("The received password was valid.  The filelist was sent.\n");  
}


std::list<std::string> Server::file_return()
{
  std::list<std::string> mylist;
  /*
   * http://stackoverflow.com/questions/612097/how-can-i-get-a-list-of-files-in-a-directory-using-c-or-c
   * This small snip of code the second option show on this webpage.
   */
  DIR *dpdf;
  struct dirent *epdf;

  dpdf = opendir("./testfolder");
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

std::string Server::wait_open_create()
{
  std::string filename;
  std::string message;
  std::string send_message;
  bool run = true;

  // wait to receive an open message
   while(run)
    {
      printf("%d: Waiting for open command...\n",serv_sock->id);

      // call receive to get the open message
      // receive
      message = serv_sock->serv_recv();
      if(message.compare("")==0)
	run = false;
      
      printf("%d: Here is the message: %s\n", serv_sock->id, message.c_str());

      // if the open message was received exit while
      // create a message handler for the received message
      MessageHandler mh(message, serv_sock);


      // for the opening of a file

      bool open = mh.key.compare("OPEN")==0;
      bool create = mh.key.compare("CREATE")==0;

      
      if(open||create)
	{
	  printf("%d: An open or receive message was received. ",serv_sock->id);

	  bool found = false;
	  std::list<std::string> filelist = file_return();
	  // iterate through the list comparing each filename to the provided
	  for(std::list<std::string>::iterator it = filelist.begin(); it != filelist.end(); it++)
	    { 
	      if(mh.name.compare(*it)==0)
		found = true;
	    }
	  
	  if(((found == true)&&open)||((found==false)&&create))
	    {
	      // no error in message send spreadsheet
	      printf("The message was valid.\n ");
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
	  filename = mh.name;
	}
       else if ((mh.key.compare("DISCONNECT")==0))
	{
	  printf("A DISCONNECT command was received close the socket.\n");
	  close(serv_sock->id);
	}
      else
	{ 
	  // send
	  send_message = mh.Error("Not an open command.");
	  serv_sock->serv_send(send_message);
	  printf("%d: Not an open command.  An error message was sent.\n",serv_sock->id);
	}
    }// end of while waiting for the open command
   
   // if it is an open command send the update right away
   //if (open)
   //{
       std::map<std::string,std::string> fakemap;
       fakemap.insert(std::pair<std::string,std::string>("A1","hello"));
       send_message = MessageHandler::Update(2, fakemap);
       serv_sock->serv_send(send_message);
       printf("The update command was sent.\n");
       //}
       //else
       // {
       // create command
       // make sure the file is created then send the update command 

       //}
  // return the filename of the spreadsheet to be opened an edited
  return filename;

}
