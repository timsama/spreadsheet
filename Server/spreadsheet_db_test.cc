#include "spreadsheet_db.h"

int main() {

  sss::spreadsheet_db test("database");

  int oldversion = test.get_version();
  std::cout << "Version is " << test.get_version() << std::endl;

  test.enter("A", "1");
  test.enter("A", "2");
  test.enter("A", "3");
  test.enter("B", "bee");
  test.enter("B", "believe");
  std::string cell;
  std::string contents;

  test.undo(&cell, &contents);
  std::cout << "TEST: " << cell << " " << contents << std::endl;
  test.undo(&cell, &contents);
  std::cout << "TEST: " << cell << " " << contents << std::endl;
  test.undo(&cell, &contents);
  std::cout << "TEST: " << cell << " " << contents << std::endl;
  test.undo(&cell, &contents);
  std::cout << "TEST: " << cell << " " << contents << std::endl;
  test.undo(&cell, &contents);
  std::cout << "TEST: " << cell << " " << contents << std::endl;

  std::cout << "Made 10 changes." << std::endl;

  std::cout << "Version should be " << oldversion+10 << ", is " << test.get_version() << std::endl;

  test.undo(&cell, &contents);
  std::cout << "TEST: " << cell << " " << contents << std::endl;

  std::cout << "Version should be " << oldversion+11 << ", is " << test.get_version() << std::endl;

}
