/*
 *MessageHandler.cc
 */

#include "MessageHandler.h"

// constructor
MessageHandler::MessageHandler(std::string message, Serv_Sock* s){
  // save the socket number
  socket = s;

  // initialize other values to empty strings
  password = "";
  name = "";
  version = "";
  cell = "";
  content = "";

  // get the keyword of the message
  key = getNextToken(message);

  if(key.compare("PASSWORD") == 0){
    password = getNextToken(message);
  }
  if(key.compare("OPEN") == 0){
    name = getNextToken(message);
  }
  if(key.compare("CREATE") == 0){
    name = getNextToken(message);
  }
  if(key.compare("RESYNC") == 0){
    // nothing to do here
  }
  if(key.compare("ENTER") == 0){
    version = getNextToken(message);
    cell = getNextToken(message);
    content = getNextToken(message);
  }
  if(key.compare("UNDO") == 0){
    version = getNextToken(message);
  }
  if(key.compare("SAVE") == 0){
    version = getNextToken(message);
  }
  if(key.compare("DISCONNECT") == 0){
    // nothing to do here
  }

  // make sure to always consume the whole string because of static integers in getNextToken
  std::string token = key;
  while(token.compare("") != 0){
    token = getNextToken(message);
  }

  // DEBUG: outputs the contents of each of the MessageHandler's data members
  //std::cout << "Key: '" << key << "', Socket: '" << socket << "', Password: '" << password << "', Name: '" << name << "', Version: '" << version << "', Cell: '" << cell << "', Content: '" << content << "'\n\n";
}

// private helper function to get the next token from the message
std::string MessageHandler::getNextToken(std::string message){
  static int start;
  int end = 1;
  char esc = static_cast<char>(27);

  // loop until the end of the token is reached
  while((start + end) < message.length()){
    while((message[start + end] != esc) && ((start + end) < message.length())){
      end++;
    }

    // declare the return string
    std::string retval;

    // copy the correct substring out of the string
    retval = message.substr(start, end);

    // start where we left off the next time through
    start += end + 1;

    // return the string
    return retval;
  }

  // return the End of String signal
  start = 0;
  return "";
}

// formats an error message for the client
std::string MessageHandler::Error(std::string errormessage){
  // initialize the message
  std::string retval = "ERROR";
  char esc = static_cast<char>(27);

  // add the error message
  retval += esc;
  retval += errormessage;

  // end the message with a newline
  retval += "\n";
  return retval;
}

// formats an invalid password message for the client
std::string MessageHandler::Invalid(){
  return "INVALID\n";
}

// formats a filelist message for the client
std::string MessageHandler::Filelist(std::list<std::string> filenames){
  // initialize the message
  std::string retval = "FILELIST";
  char esc = static_cast<char>(27);

  // iterate through the list, adding each filename to the return string
  for(std::list<std::string>::iterator it = filenames.begin(); it != filenames.end(); it++){
    retval += esc;
    retval += *it;
  }

  // end the message with a newline
  retval += "\n";
  return retval;
}

// formats a saved message for the client
std::string MessageHandler::Saved(){
  return "SAVED\n";
}

std::string MessageHandler::Update(int version, std::map<std::string, std::string> cells){
  std::string retval = "UPDATE";
  char esc = static_cast<char>(27);
  retval += esc;

  // add the version
  std::stringstream ss;
  ss << version;
  retval += ss.str();

  retval += Cells(cells);

  return retval;
}

// formats a sync message for the client
std::string MessageHandler::Sync(int version, std::map<std::string, std::string> cells){
  std::string retval = "SYNC";
  char esc = static_cast<char>(27);
  retval += esc;

  // add the version
  std::stringstream ss;
  ss << version;
  retval += ss.str();
 
  retval += Cells(cells);

  return retval;
}

std::string MessageHandler::Cells(std::map<std::string, std::string> cells) {
 // initialize the message
  std::string retval = "";
  char esc = static_cast<char>(27);

  // iterate through the map, adding each cell name and contents to the return string
  for(std::map<std::string, std::string>::iterator it = cells.begin(); it != cells.end(); it++){
    retval += esc;
    retval += it->first;
    retval += esc;
    retval += it->second;
  }

  // end the message with a newline
  retval += "\n";
  return retval;
}

// formats an update message for the client
std::string MessageHandler::Update(int version, std::string cell, std::string content){
  // initialize the message
  std::string retval = "UPDATE";
  char esc = static_cast<char>(27);
  retval += esc;

  // add the version
  std::stringstream ss;
  ss << version;
  retval += ss.str();
  retval += esc;

  // add the cell and contents
  retval += cell;
  retval += esc;
  retval += content;

  // end the message with a newline
  retval += "\n";
  return retval;
}
