/* parsecells.cc
 *
 */

#include "parsecells.h"

parsecells::parsecells() { }

bool parsecells::is_letter(char s) {
  return ((s >= 'a' && s <= 'z') || (s >= 'A' || s <= 'Z'));
}

bool parsecells::is_number(char s) {
  return (s >= '0' && s <= '9');
}

std::set<std::string> parsecells::parse(std::string content) {
  std::set<std::string> cells;
  std::string temp = "";
  
  // check to see if the content begins with an = sign
  if(content[0] == '=') {
    // start walking the string to find letters
    for(int i = 1; i < content.length(); i++) {
      if(is_letter(content[i]) == true) {
	temp.append(&content[i]);
	i++;
	// Add numbers after the letter
	while(i < content.length() && is_number(content[i])) {
	  temp.append(&content[i]);
	  i++;
	} 
	cells.insert(temp);
	temp = "";
      }
    }
  }
  return cells;
}
