/*
 *SS_Server.h
 */

#ifndef SS_SERVER_H
#define SS_SERVER_H

#include "MessageHandler.h"
#include "Serv_Sock.h"
#include <string>
#include <iostream>
#include <vector>
#include <map>
#include <algorithm>
#include <set>
#include <queue>
#include <stdio.h>
#include <sys/types.h>
#include <sys/socket.h>




class SS_Server{

 public:
  SS_Server();
  SS_Server(std::string filename, Serv_Sock sock);
  ~SS_Server();

  void server_loop();  
  void socket_loop(Serv_Sock sock);

 private:
  //std::mutex mtx;
  int socket;
  Serv_Sock serv_sock;

  void broadcast(std::string message);
  void broadcast(std::string message, Serv_Sock sock);

  //Spreadsheet ss;
  std::string ss;
  // queue of MessageHandlers
  std::queue<MessageHandler> messages;
  std::set<Serv_Sock*> sockets;
 
};

#endif
