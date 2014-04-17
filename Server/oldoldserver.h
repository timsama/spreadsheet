/*
 *Server.h
 */

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

#include "SS_Server.h"
#include <string>
#include <iostream>
#include <vector>
#include <map>

#define PORT "2500"  // the port users will be connecting to

#define BACKLOG 10     // how many pending connections queue will hold


class Server{
 public:
  Server(int sock);
  ~Server();

  // call all of the necessary functions to handle the client and pass it to the SS_Server
  void handle_client();

  // map to store spreadsheets currently being editted 
  std::map<std::string, SS_Server*> open_spreads;

 private:
  int socket;
  
  char buffer[1024];
  
  std::string send_message;
  std::string delimiter;
  std::string endline;
  std::vector<std::string> filelist;

  void wait_authorize();
  std::string wait_open_create();
  void check_open_spreads(std::string filename);
  void serv_send(int sock, std::string message);
  std::string serv_recv(int sock);
};
