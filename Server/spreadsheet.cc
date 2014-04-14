
#include "spreadsheet.h"

namespace sss {

  // Constructor, takes a pointer to a
  // sqlite3 database connection
  spreadsheet::spreadsheet(sqlite3 *db) {
    this->db = db;
  }
  
  // Destructor
  spreadsheet::~spreadsheet() {
    // Nothing to do here yet
  }
  
  
  // Returns 0 if circular dependency formed
  //  or >0 new version number
  int spreadsheet::enter(std::string cell, std::string contents) {
    
    std::set<std::string> contents_cells = get_cells(contents);

    if(free_from_circular(cell, contents_cells)) {
      
      // Update contents
      
      return this->version;
    } else {
      return 0;
    }
    
  }
  
  // Returns 0 if unable to undo
  //  or >0 new version number
  int spreadsheet::undo() {
    
  }
  
  std::set<std::string> spreadsheet::get_cells(std::string contents) {
    // Parse string

    // Build set

    // Return set
  }

  bool spreadsheet::free_from_circular(std::string cell, std::set<std::string> content_cells) {

    std::queue<std::string> cells;
    
    // Add contents to cells
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
    
    // The dependency graph was a DAG
    return true;  
  }
  
}
