#include "spreadsheet.h"

int main() {
  
  sss::spreadsheet s("expenses");

  std::cout << "Starting with version " << s.get_version() << std::endl << std::endl;

  std::cout << "Version " << s.enter("A1", "=A2") << std::endl << std::endl;
  // Should form a circular dependency
  std::cout << "Version " << s.enter("A2", "=A1") << std::endl << std::endl;

  std::string cell;
  std::string contents;
  std::cout << "Version " << s.undo(&cell, &contents);
  std::cout << " -- Undo to " << cell << "=" << contents << std::endl;
  
}


