/*
 *SS_Server.h
 */

#ifndef SS_SERVER_H
#define SS_SERVER_H

#include "MessageHandler.h"
#include "Serv_Sock.h"
#include "spreadsheet.h"
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
#include <unistd.h>
#include <boost/lexical_cast.hpp>
#include <boost/thread/thread.hpp>



class SS_Server{

 public:

  SS_Server(std::string filename);
  ~SS_Server();

  void socket_loop_thread(Serv_Sock* sock);
  void server_loop_thread();
  void add_sock(Serv_Sock* sock);

 private:

  void server_loop();  
  void socket_loop(Serv_Sock* sock);

  void broadcast(std::string message);
  void broadcast(std::string message, Serv_Sock* sock);
  void disconnect(Serv_Sock* sock);

  sss::spreadsheet ss;
  // queue of MessageHandlers
  std::queue<MessageHandler> messages;
  std::set<Serv_Sock*> sockets;
 
  boost::mutex guard;

};

#endif
