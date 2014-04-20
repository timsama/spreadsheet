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
    if(free_from_circular(cell, contents)) {      
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

    std::queue<std::string> cells;
    
    // Add content_cells to cells
    for (std::set<std::string>::iterator it = content_cells.begin(); it != content_cells.end(); ++it) {

      // Circular dependency found if a child references the parent
      if(*it == cell) {
	return false;
      } else {
	cells.push(*it);
      }
    }
    
    // Go through the queue
    while(!cells.empty()) {
      std::string current_cell = cells.front();
      cells.pop();
      
      // Add current_cell's dependencies to the queue
      std::set<std::string> deps = this->dependencies.get_dependents(current_cell);
      for (std::set<std::string>::iterator it = deps.begin(); it != deps.end(); ++it) {

	// Circular dependency found if a child references the parent
	if(*it == cell) {
	  return false;
	} else {
	  cells.push(*it);
	}
      }
    }

    // Things look good!
    dependencies.replace_dependents(cell, content_cells);    

    // The dependency graph was a DAG
    return true;  
  }
  
}
