/*
 *messagehandler definitions
 */
#include "messagehandler.h"
#include <iostream>
#include <string>
#include <vector>


//the constructor of the messagehand
messagehandler::messagehandler()
{
  messagehandler::pass = "warrior";
}

std::string messagehandler::get_pass()
{
  return pass;
}

//to be filled in shortly
bool messagehandler::is_valid(std::string str)
{
  return false;
}


std::string messagehandler::incoming_message(std::string message)
{
  std::string delimiter = ":";
  std::string endline = "\n";
  std::string token = message.substr(0,message.find(delimiter));
  std::string content = message.substr(message.find(delimiter)+1,(message.length()-token.length()-2));
 
  //for the password
  if(token == "PASSWORD")
    {      
      if( content == "warrior")
        {
	  std::cout<<"Correct Password"<<std::endl;
        }
      else
        {
	  std::cout<<"Wrong Password please try again"<<std::endl;      
        }
    
    }
  //
  //for the opening of a file
  else if(token == "OPEN")
    {
      std::vector<std::string> list;
      list.push_back("ss1");
      list.push_back("ss2");
      list.push_back("ss3");
      std::string *p;
      bool found = false;
      for(int i = 0; i < list.size();i++)
        {
          if(list[i] == content)
            found = true;           
        }
      if(found == true)
	std::cout<<"spreadsheet found go to command to open"<<std::endl;
      else
	std::cout<<"ERROR FILE NOT FOUND"<<std::endl;
    
    }
  //
  //for creating the file
  else if(token == "CREATE")
    {
      std::vector<std::string> list;
      list.push_back("ss1");
      list.push_back("ss2");
      list.push_back("ss3");
      std::string *p;
      bool found = false;
      for(int i = 0; i < list.size();i++)
        {
          if(list[i] == content)
            found = true;           
        }
      if(found == true)
	std::cout<<"ERROR Spreadsheet name already exist, please enter another."<<std::endl;
      else
	std::cout<<"Name is valid 'call correct command for new spreadsheet'."<<std::endl;
    }
  else if(token == "SAVE")
    {
      //check to make sure the client current version is the same as the servers before a save is committed
      //then check the save
    }
  else if(token == "ENTER")
    {
      std::string version = content.substr(0,content.find(delimiter));
      std::string current = content.substr(content.find(delimiter)+1,(content.length()-token.length()-2));
      //std::cout<<version<<std::endl;
      std::string cellname = current.substr(0,current.find(delimiter));;
      //std::cout<<cellname<<std::endl;
      std::string cellcontent = current.substr(current.find(delimiter)+1,(current.length()-cellname.length()-1));
      //std::cout<<cellcontent<<std::endl;
      
      //check the cellcontent to make sure its correct
      if(is_valid(cellcontent))
	std::cout<<"the content is valid"<<std::endl;
      else
	std::cout<<"The content was not correct please enter another number"<<std::endl;
    }
  else if(token == "UNDO")
    {
      //check to make sure there version is all so the current version if so then send back the current version
    }
  else if(token == "DISCONNECT")
    {

    }
  else if(token == "RESYNC")
    {
      //send the client the current version to update all the cells
    }

}

int main()
{
  messagehandler m;
  m.incoming_message("PASSWORD:Warrior\n");
  //m.incoming_message("OPEN:ss4\n");
  m.incoming_message("ENTER:version:A10:2x+6\n");
}
