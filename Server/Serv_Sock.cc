#include "Serv_Sock.h"

Serv_Sock::Serv_Sock(int sock)
{
  id = sock;
}

Serv_Sock::Serv_Sock()
{
  id = 0;
}

void Serv_Sock::serv_send(std::string message)
{
  const char * e;
  int n;
  int sock = this->id;

  e = message.c_str();
  n = send(sock,e,message.length(),0);
  if (n < 0) 
    perror("ERROR writing to socket");  
}

std::string Serv_Sock::serv_recv()
{
  int n;
  char buffer[1024];
  int sock = this->id;
 
  bzero(buffer,1024);

  n = recv(sock,buffer,1023,0);
      
  // check if the client has disconnected
  if (n==0)
    {
      printf("Closed in while loop in wait_open_create\n");
      close(sock);
      //break;
    }
  if (n < 0)
    {
      perror("ERROR reading from socket");
      // break;
    }
  
  std::string return_string(buffer);

  return return_string;
}
