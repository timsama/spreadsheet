#include "spreadsheet.h"

int main() {
  
  sss::spreadsheet s("expenses");

  std::cout << "Starting with version " << s.get_version() << std::endl;

  std::cout << "Version " << s.enter("A1", "Hello World") << std::endl;
  std::cout << "Version " << s.enter("A1", "Goodbye") << std::endl;

  std::string cell;
  std::string contents;
  std::cout << "Version " << s.undo(&cell, &contents);
  std::cout << " -- Rollback to " << cell << "=" << contents << std::endl;
  
}


