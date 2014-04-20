/*
 *Server.h
 */
#ifndef SERVER_H
#define SERVER_H


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

#include "Serv_Sock.h"
#include "SS_Server.h"
#include <string>
#include <iostream>
#include <vector>
#include <map>
#include <list>
#include "dirent.h"
#define PORT "2500"  // the port users will be connecting to

#define BACKLOG 10     // how many pending connections queue will hold


class Server{
 public:
  Server(Serv_Sock* sock);
  ~Server();

  // call all of the necessary functions to handle the client and pass it to the SS_Server
  void handle_client();

  // map to store spreadsheets currently being editted 
  std::map<std::string, SS_Server> open_spreads;

 private:
  int socket;
  Serv_Sock* serv_sock;
  std::list<std::string> file_return();
  
  void wait_authorize();
  std::string wait_open_create();
  void open_spreadsheet(std::string filename);
};
#endif
