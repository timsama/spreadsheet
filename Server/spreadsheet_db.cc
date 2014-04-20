#include "spreadsheet_db.h"
#include <sstream>
#include <iostream>

namespace sss {

  spreadsheet_db::spreadsheet_db(std::string name) {
    this->name = name;
    open_db();
    init_table();
  }

  spreadsheet_db::~spreadsheet_db() {
    sqlite3_close(this->db);
  }

  int spreadsheet_db::open_db() {
    // Open the database file
    int error = sqlite3_open(("spreadsheets/" + this->name).c_str(), &this->db);
    // Turn on extended error codes (for debugging)
    sqlite3_extended_result_codes(this->db, 1);
    if(error != SQLITE_OK) {
      std::cout << "Error: couldn't open " << this->name << ". SQLite error: " << sqlite3_errmsg(this->db) << std::endl;
      return -1;
    } 
  }

  int spreadsheet_db::init_table() {
    // Create transactions table
    int error = sqlite3_exec(this->db, 
			     "CREATE TABLE IF NOT EXISTS transactions (cell TEXT, contents TEXT, active INTEGER);"
			     "CREATE INDEX IF NOT EXISTS index_cell ON transactions (cell);"
			     "CREATE INDEX IF NOT EXISTS index_active ON transactions (active);"
			     "CREATE INDEX IF NOT EXISTS index_cell_active ON transactions (cell, active);"
			     "CREATE TABLE IF NOT EXISTS data (key TEXT UNIQUE, value INTEGER);"
			     "INSERT OR IGNORE INTO data (key, value) VALUES ('version', 1);",
			     NULL, NULL, NULL);
    if(error != SQLITE_OK) {
      std::cout << "Error: couldn't create transactions table. SQLite error: " << error << std::endl;
      return -1;
    }
    
    // Table created
    return 0;
  }
  
  int64_t spreadsheet_db::get_version() {
    sqlite3_stmt *statement;
    int result = 0;
    std::string sql = "";
    
    // Get the version
    int64_t version;
    sql = "SELECT value FROM data WHERE key = 'version';";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);
    if(result == SQLITE_OK) {
      if(sqlite3_step(statement)) {
	version = sqlite3_column_int64(statement, 0);
      } else {
	version = 0;
      }
    } else {
      version = 0;
    }
    sqlite3_finalize(statement);    

    return version;
  }

  int64_t spreadsheet_db::increase_version() {
    sqlite3_stmt *statement;
    int result = 0;
    std::string sql = "";
    
    // Get the new version
    int64_t version = get_version() + 1;

    sql = "INSERT OR REPLACE INTO data (key, value) VALUES ('version', ?);";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);
    if(result == SQLITE_OK) {
      // Bind
      sqlite3_bind_int64(statement, 1, version);
      // Commit
      sqlite3_step(statement);
    } else {
      version = 0;
    }
    sqlite3_finalize(statement);    

    return version;
  }

  int64_t spreadsheet_db::undo(std::string *cell_out, std::string *contents_out) {
    sqlite3_stmt *statement;
    int result = 0;
    std::string sql = "";

    // Get latest rowid and cell
    std::string rowid = "";
    std::string cell = "";    
    sql = "SELECT MAX(rowid), cell FROM transactions";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);
    if(result == SQLITE_OK && sqlite3_step(statement) == SQLITE_ROW && sqlite3_column_type(statement, 0) != SQLITE_NULL) {
      rowid = std::string(reinterpret_cast<const char*>(sqlite3_column_text(statement, 0)));
      cell = std::string(reinterpret_cast<const char*>(sqlite3_column_text(statement, 1)));
    } else {
      // Cell doesn't exist, table empty!
    }
    // Cleanup
    sqlite3_finalize(statement);

    // Update the table
    if(rowid != "") {
      sql = "DELETE FROM transactions WHERE rowid = '" + rowid + "'; " +
	"UPDATE transactions SET active = 1 WHERE rowid IN (SELECT MAX(rowid) FROM transactions WHERE cell = '" + cell + "'); ";
      result = sqlite3_exec(this->db, sql.c_str(), NULL, NULL, NULL);
      // No need to finalize with exec
    }

    // Setup out params
    sql = "SELECT cell, contents FROM transactions WHERE active = 1 AND cell = ?;";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);

    *cell_out = cell;    
    if(result == SQLITE_OK) {
      sqlite3_bind_text(statement, 1, cell.c_str(), strlen(cell.c_str()), 0);
      if(sqlite3_step(statement) != SQLITE_DONE) {
	*contents_out = std::string(reinterpret_cast<const char*>(sqlite3_column_text(statement, 1)));  
      } else {
	*contents_out = std::string("");
      } 

    } else {
      // Statement didn't work 
    }

    return increase_version();
  }

  int64_t spreadsheet_db::enter(std::string cell, std::string contents) {
    sqlite3_stmt *statement;
    int result = 0;
    std::string sql = "";
    
    // Deactivate all previous data for the cell
    sql = "UPDATE transactions SET active = 0 WHERE cell = ? AND active = 1";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);
    if(result == SQLITE_OK) {
      // Bind values
      sqlite3_bind_text(statement, 1, cell.c_str(), strlen(cell.c_str()), 0);
      // Commit
      sqlite3_step(statement);
      sqlite3_finalize(statement);
    } else {
      std::cout << "There was an error preparing the statement: " << result << " " << sqlite3_errmsg(db) << std::endl;
      return 0;
    } 

    // Add the new data
    sql = "INSERT INTO transactions (cell, contents, active) VALUES (?,?,1)";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);
    if( result == SQLITE_OK ) {
      // Bind values
      sqlite3_bind_text(statement, 1, cell.c_str(), strlen(cell.c_str()), 0);
      sqlite3_bind_text(statement, 2, contents.c_str(), strlen(contents.c_str()), 0);
      // Commit
      sqlite3_step(statement);
    } else {
      std::cout << "There was an error preparing the statement: " << result << " " << sqlite3_errmsg(db) << std::endl;
      return 0;
    }
    sqlite3_finalize(statement);  

    return increase_version();
  }
  
  std::string spreadsheet_db::get_name() {
    return this->name;
  }
  
  std::map<std::string, std::string> spreadsheet_db::get_cells() {
    sqlite3_stmt *statement;
    int result = 0;
    std::string sql = "";
   
    std::string cell, contents;
    std::map<std::string, std::string> cells = std::map<std::string, std::string>();
    
    // Query for active cells
    sql = "SELECT cell, contents FROM transactions WHERE active = 1 AND contents != '';";
    result = sqlite3_prepare_v2(this->db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);
    if(result == SQLITE_OK) {
      // Loop
      while(sqlite3_step(statement) && sqlite3_column_type(statement, 0) != SQLITE_NULL) {
	// Add to map
	cell = std::string(reinterpret_cast<const char*>(sqlite3_column_text(statement, 0)));
	contents = std::string(reinterpret_cast<const char*>(sqlite3_column_text(statement, 1)));
	cells.insert(std::pair<std::string, std::string>(cell, contents));
      }
    }

    sqlite3_finalize(statement);    

    // Return map
    return cells;
  }
}
