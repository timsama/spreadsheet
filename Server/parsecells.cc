/* parsecells.cc
 *
 */

#include "parsecells.h"
#include <iostream>
#include <sstream>

parsecells::parsecells() { }

bool parsecells::is_letter(char s) {
  return ((s >= 'a' && s <= 'z') || (s >= 'A' || s <= 'Z'));
}

bool parsecells::is_number(char s) {
  return (s >= '0' && s <= '9');
}

std::string converter(char c) {
  //CONVERTING THE CHARACTER TO A STRING
  std::stringstream ss;
  std::string s;;
  ss << c;
  ss >> s;
  return s;
}

std::set<std::string> parsecells::parse(std::string content) {
  std::set<std::string> cells;
  std::string temp = "";
  
  // check to see if the content begins with an = sign
  if(content[0] == '=') {
    // start walking the string to find letters
    for(int i = 1; i < content.length(); i++) {
      if(is_letter(content[i])) {
	temp += converter(content[i]);
	i++;
	// Add numbers after the letter
	while(i < content.length() && is_number(content[i])) {
	  temp += converter(content[i]);
	  i++;
	} 
	cells.insert(temp);
	std::cout << " added " << temp << std::endl;
	temp = "";
      }
    }
  }
  return cells;
}
