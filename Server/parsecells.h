/* parsecells.h
 *
 */

#include <set>
#include <string>

#ifndef PARSECELLS_H
#define PARSECELLS_H

class parsecells {

 private:
  static bool is_letter(char s);
  static bool is_number(char s);
 public:
  parsecells();
  static std::set<std::string> parse(std::string content);

};

#endif
