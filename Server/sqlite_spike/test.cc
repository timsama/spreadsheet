#include <iostream>
#include <sqlite3.h>
#include <stdint.h>
#include <cstring>

using namespace std;

int database_open(string filename, sqlite3 **db);
int64_t spreadsheet_enter(sqlite3 *db, string spreadsheet, string cell, string contents);

int main (int argc, const char* argv[]) {

  sqlite3 *db;
  
  database_open("spreadsheets.sqlite", &db);
  cout << spreadsheet_enter(db, "Test", "A1", "Hello World") << endl;

  sqlite3_close(db);

}

int database_open(string filename, sqlite3 **db) {

  // Open the database file
  int error = sqlite3_open(filename.c_str(), db);
  // Turn on extended error codes (for debugging)
  sqlite3_extended_result_codes(*db, 1);
  if(error != SQLITE_OK) {
    cout << "Error: couldn't open " << filename << ". SQLite error: " << sqlite3_errmsg(*db) << endl;
    return -1;
  } 
 
  // Create transactions table
  error = sqlite3_exec(*db, 
		       "CREATE TABLE IF NOT EXISTS transactions (spreadsheet TEXT, cell TEXT, contents TEXT);"
                       "CREATE INDEX IF NOT EXISTS index_spreadsheet ON transactions (spreadsheet);"
		       "CREATE INDEX IF NOT EXISTS index_cell ON transactions (cell);",
		       NULL, NULL, NULL);
  if(error != SQLITE_OK) {
    cout << "Error: couldn't create transactions table. SQLite error: " << error << endl;
    return -1;
  }

  // Table created
  return 0;
}

int64_t spreadsheet_enter(sqlite3 *db, string spreadsheet, string cell, string contents) {
  if(!db) return -1;

  sqlite3_stmt *statement;
  
  string sql = "INSERT INTO transactions (spreadsheet, cell, contents) VALUES (?,?,?)";
  int result = sqlite3_prepare_v2(db, sql.c_str(), strlen(sql.c_str()), &statement, NULL);

  if( result == SQLITE_OK ) {
    // Bind values
    sqlite3_bind_text(statement, 1, spreadsheet.c_str(), strlen(spreadsheet.c_str()), 0);
    sqlite3_bind_text(statement, 2, cell.c_str(), strlen(cell.c_str()), 0);
    sqlite3_bind_text(statement, 3, contents.c_str(), strlen(contents.c_str()), 0);

    // Commit
    sqlite3_step(statement);
    sqlite3_finalize(statement);

    // Return new rowid (useful as a verison number)
    return sqlite3_last_insert_rowid(db);
  } else {
    cout << "There was an error preparing the statement: " << result << " " << sqlite3_errmsg(db) << endl;
    return 0;
  } 
    
}
