/*
 *MessageHandler.h
 */
#include "Serv_Sock.h"
#include <string>
#include <map>
#include <list>
#include <sstream>
#include <algorithm>

class MessageHandler
{
 public:
  // constructor
  MessageHandler();
  MessageHandler(std::string message, Serv_Sock* s);

  // when instantiated (received messages), MessageHandler is treated like a struct--all data members are public

  std::string key; // contains the keyword of the received message
  std::string password; // contains either the password (for PASSWORD messages), or an empty string
  std::string name; // contains either the spreadsheet name (for OPEN and CREATE messages), or an empty string
  std::string version; // contains the version number (for ENTER, UNDO, and SAVE messages)
  std::string cell; // contains the cell's name (for ENTER messages), or an empty string
  std::string content; // contains the cell's contents (for ENTER messages), or an empty string
  Serv_Sock* socket; // contains the socket number associated with the message

  // when using static methods (sending messages), just return a correctly-formatted string

  static std::string Error(std::string errormessage); // formats an error message for the client
  static std::string Invalid(); // formats an invalid password message for the client
  static std::string Filelist(std::list<std::string> filenames); // formats a file list message for the client
  static std::string Saved(); // formats a saved message for the client
  static std::string Sync(int version, std::map<std::string, std::string> cells); // formats a sync message for the client
  static std::string Update(int version, std::map<std::string, std::string> cells);
  static std::string Update(int version, std::string cell, std::string content); // formats an update message for the client
  static const char* readable(std::string withesc);

 private:
  static std::string Cells(std::map<std::string, std::string> cells);
  static std::string getNextToken(std::string message); // gets the next token from the message, returns "" when done
};
