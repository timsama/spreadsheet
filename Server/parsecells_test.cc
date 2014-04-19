#include "parsecells.h"
#include <set>
#include <string>
#include <iostream>

int main() {

  std::set<std::string> cells = parsecells::parse("=A2+B32+c4");

  std::cout << "THe set has " << cells.size() << " items." << std::endl; 


}
