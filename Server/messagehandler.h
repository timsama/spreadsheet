/*
 *The message handler for the class 
 */
#ifndef MESSAGEHANDLER
#define MESSAGEHANDLER
#include <iostream>

class messagehandler
{


 private:

  std::string pass;


 public:

  //constructor
  messagehandler();

  //getting the set password 
  std::string get_pass();

  //an isvalid function
  bool is_valid(std::string str);

  //function to help parse the correct message
  std::string incoming_message(std::string message);






};
#endif
