/*
 *Serv_Sock.h - wrapper class for socket
 */

#ifndef SERV_SOCK_H 
#define SERV_SOCK_H

#include <sys/types.h>
#include <sys/socket.h>
#include <string.h>
#include <stdio.h>
#include <string>
#include <iostream>

class Serv_Sock{

 private:

 public:
  Serv_Sock(int sock);
  Serv_Sock();
  int id;

  void serv_send(std::string message);
  std::string serv_recv();
  
};

#endif
