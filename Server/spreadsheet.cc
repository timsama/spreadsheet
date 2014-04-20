#include "spreadsheet.h"
#include "spreadsheet_db.h"
#include "parsecells.h"

namespace sss {

  // Constructor, takes a pointer to a
  // sqlite3 database connection
  spreadsheet::spreadsheet(std::string name)
    : ssdb(name)
  {
    this->name = name;
    //this->ssdb = spreadsheet_db(this->name);
  }
  
  // Destructor
  spreadsheet::~spreadsheet() {
    // Nothing yet...
  }

  int spreadsheet::get_version() {
    return this->ssdb.get_version();
  }

  std::map<std::string, std::string> spreadsheet::get_cells() {
    return this->ssdb.get_cells();
  }
  
  // Returns
  //  -1 if circular dependency formed
  //   0 if error adding to model
  //  >0 new version number
  int spreadsheet::enter(std::string cell, std::string contents) {
    std::cout << cell << " = " << contents << std::endl;
    if(free_from_circular(cell, contents)) {      
      // Add the cells dependencies
      dependencies.replace_dependents(cell, parsecells::parse(contents));
      return this->ssdb.enter(cell, contents);
    } else {
      // Circular dependency
      return -1;
    }
  }
  
  // Returns 0 if error updating database
  //  or >0 new version number
  int spreadsheet::undo(std::string *cell, std::string *contents) {
    int version = this->ssdb.undo(cell, contents);
    if(*cell != "") {
      free_from_circular(*cell, *contents);
    }
    return version;
  }

  // Utility to manage the dependency graph
  // checks to see if contents will result in a circular dependency
  // if they do, returns false
  // if they don't, adds cells to graph and returns true
  bool spreadsheet::free_from_circular(std::string cell, std::string contents) {

    // Use the parser to actually get cells
    std::set<std::string> content_cells = parsecells::parse(contents);

    std::cout << "Found " << content_cells.size() << " cells in the formula." << std::endl;

    std::queue<std::string> cells;
    
    std::cout << "Checking cells against " << cell << std::endl;

    // Add content_cells to cells
    for (std::set<std::string>::iterator it = content_cells.begin(); it != content_cells.end(); ++it) {

      // Circular dependency found if a child references the parent
      if(*it == cell) {
	std::cout << " !!! Found circular child " << *it << std::endl;
	return false;
      } else {
	cells.push(*it);
	std::cout << " Adding child " << *it << std::endl;
      }
    }
    
    // Go through the queue
    while(!cells.empty()) {
      std::string current_cell = cells.front();
      cells.pop();
      
      // Add current_cell's dependencies to the queue
      std::cout << " Recursively checking " << current_cell << std::endl;
      std::set<std::string> deps = this->dependencies.get_dependents(current_cell);
      for (std::set<std::string>::iterator it = deps.begin(); it != deps.end(); ++it) {

	// Circular dependency found if a child references the parent
	if(*it == cell) {
	  std::cout << " !!! Found circular child " << *it << std::endl;
	  return false;
	} else {
	  cells.push(*it);
	  std::cout << " Adding child " << *it << std::endl;
	}
      }
    }
    
    // The dependency graph was a DAG
    return true;  
  }
  
}
