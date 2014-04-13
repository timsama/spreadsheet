
#ifndef SPREADSHEET_H
#define SPREADSHHET_H

#include <string>
#include <set>
#include <queue>
#include <sqlite3.h>

namespace spreadsheet {

  enum command_type {
    ENTER, UNDO
  };

  struct command {
    int fromSocket;
    command_type command;
    std::string version;
    std::string cell;
    std::string contents;
  };

  class spreadsheet {
    
  private:
    std::string name;
    dg dependencies;
    std::set<int> sockets;
    std::queue<command> commands;
    sqlite3 *db;

    void broadcast(string message);
    void broadcast(string message, int socket);

    

  public:
    spreadsheet(sqlite3 *db);
    ~spreadsheet();
  
    void add_socket(int socket);
    void enqueue_command(int socket, command_type command, std::string version, std::string cell, std::string contents);
  };

}

#endif
