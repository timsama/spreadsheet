#include "spreadsheet.h"

int main() {
  
  sss::spreadsheet s("expenses");

  int version = s.get_version();
  version++;

  if(s.enter("A1", "=A2") == version++) std::cout << "PASSED" << std::endl;
  else std::cout << "Wrong version.";

  if(s.enter("A2", "=A3") == version++) std::cout << "PASSED" << std::endl;
  else std::cout << "Wrong version.";

  if(s.enter("A3", "=A4") == version++) std::cout << "PASSED" << std::endl;
  else std::cout << "Wrong version.";

  if(s.enter("A4", "=A1") == -1) std::cout << "PASSED" << std::endl;
  else std::cout << "Should have seen a circular dependency." << std::endl;

  std::string cell;
  std::string contents;
  if(s.undo(&cell, &contents) == version++) std::cout << "PASSED" << std::endl;
  else std::cout << "Undo should still increment version..." << std::endl;


  // Check cells
  std::map<std::string, std::string> result = s.get_cells();

  if(result.size() == 2) std::cout << "PASSED" << std::endl;
  else std::cout << "The spreadsheet should have contained 2 cells but contained " << result.size() << std::endl;

  if(result.find("A1") != result.end() && result.find("A2") != result.end()) {
    std::cout << "PASSED" << std::endl;
      if(result.find("A1")->second == "=A2" && result.find("A2")->second == "=A3") std::cout << "PASSED" << std::endl;
      else std::cout << "The spreadsheet didn't contain the right values for A1 and A2" << std::endl;
  } else std::cout << "The spreadsheet didn't contain A1 and A2" << std::endl;

}


