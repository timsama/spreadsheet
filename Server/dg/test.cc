#include "dependency_graph.h"

#include <iostream>

using namespace std;

int main() {
  std::cout << "Starting Dependency Graph Test" << std::endl;

  dependency_graph d;

  // Zero Size
  if(d.size() == 0) std::cout << "PASS" << std::endl;
  else std::cout << "Empty graph not size 0." << std::endl;

  // Zero Dependents
  if(!d.has_dependents("a")) std::cout << "PASS" << std::endl;
  else std::cout << "Empty graph should have no dependents." << std::endl;

  // Zero Dependees
  if(!d.has_dependees("a")) std::cout << "PASS" << std::endl;
  else std::cout << "Empty graph should have no dependees." << std::endl;

  // Zero Dependents, List Empty
  if(d.get_dependents("a").size() == 0) std::cout << "PASS" << std::endl;
  else std::cout << "Empty graph should have an empty dependents list.";

  // Zero Dependees, List Empty
  if(d.get_dependees("a").size() == 0) std::cout << "PASS" << std::endl;
  else std::cout << "Empty graph should have an empty dependees list.";

  // Zero Index
  if(d["a"] == 0) std::cout << "PASS" << std::endl;
  else std::cout << "Empty graph should return 0 when indexed.";

  // Remove a non-existent dependency
  try {
    d.remove_dependency("a", "b");
    std::cout << "PASS" << std::endl;
  } catch(std::exception &e) {
    std::cout << "Empty graph should not error when removing dependency.";
  }

  // Add dependency
  d.add_dependency("a","b");
  d.add_dependency("a","b"); // 2nd time test
  if(d["b"] != 1) {
    std::cout << "Error: b should have 1 dependee, has " << d["b"] << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }
  if(d.size() != 1) {
    std::cout << "Error: node_set should have 1 pair, has " << d.size() << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }

  // Add dependency
  d.add_dependency("c","b");
  d.add_dependency("c","b");
  if(d["b"] != 2) {
    std::cout << "Error: b should have 2 dependees, has " << d["b"] << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }
  if(d.size() != 2) {
    std::cout << "Error: node_set should have 2 pairs, has " << d.size() << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }

  // Add dependency
  d.add_dependency("d","b");
  d.add_dependency("d","b");
  if(d["b"] != 3) {
    std::cout << "Error: b should have 3 dependees, has " << d["b"] << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }
  if(d.size() != 3) {
    std::cout << "Error: node_set should have 3 pairs, has " << d.size() << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }

  // Remove dependency
  d.remove_dependency("d","b");
  d.remove_dependency("d","b");
  if(d["b"] != 2) {
    std::cout << "Error: b should have 2 dependees, has " << d["b"] << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }

  // Remove dependency
  d.remove_dependency("a","b");
  d.remove_dependency("a","b");
  if(d["b"] != 1) {
    std::cout << "Error: b should have 1 dependees, has " << d["b"] << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }

  // Remove dependency
  d.remove_dependency("c","b");
  d.remove_dependency("c","b");
  if(d["b"] != 0) {
    std::cout << "Error: b should have 0 dependees, has " << d["b"] << std::endl;
  } else {
    std::cout << "PASS" << std::endl;
  }

  // The graph should be blank now
  if(d.size() == 0) std::cout << "PASS" << std::endl;
  else std::cout << "Graph should be empty." << std::endl;

  // Add 3 dependees
  d.add_dependency("a1", "b");
  d.add_dependency("a2", "b");
  d.add_dependency("a3", "b");
  
  if(d["b"] == 3) std::cout << "PASS" << std::endl;
  else std::cout << "b didn't have 3 dependees.";

  // Replace all 3 with nothing.
  d.replace_dependees("b", std::set<std::string>());

  if(d["b"] == 0) std::cout << "PASS" << std::endl;
  else std::cout << "b should now have 0 dependees.";

  if(d.size() == 0) std::cout << "PASS" << std::endl;
  else std::cout << "After replacing dependees, the graph should be empty." << std::endl;

  // Add 3 dependents
  d.add_dependency("a", "b");
  d.add_dependency("a", "c");
  d.add_dependency("a", "d");

  if(d.has_dependents("a")) cout << "PASS" << endl;
  else cout << "a should have dependents." << endl;

  // Remove 1
  d.remove_dependency("a", "b");
  if(d.has_dependents("a")) cout << "PASS" << endl;
  else cout << "a should still have dependents." << endl;

  // Replace with nothing
  d.replace_dependents("a", set<string>());
  if(!d.has_dependents("a")) cout << "PASS" << endl;
  else cout << "after replace with nothing, a should not have dependents." << endl;

  // Replace with 1
  set<string> one;
  one.insert("x");
  d.replace_dependents("a", one);
  if(d.has_dependents("a")) cout << "PASS" << endl;
  else cout << "after replace with a set containing x, a should have dependents." << endl;
  
  // Remove x
  d.remove_dependency("a", "x");
  if(!d.has_dependents("a")) cout << "PASS" << endl;
  else cout << "after removing (a, x), a should have dependents." << endl;
 
  d.replace_dependees("a", one);
  if(d.has_dependees("a") && d.has_dependents("x") && d.size() == 1) cout << "PASS" << endl;
  else cout << "the (x, a) pair should be the only one." << endl;

  if(d.get_dependees("a") == one) cout << "PASS" << endl;
  else cout << "a doesn't have the right dependees." << endl;

  set<string> two;
  two.insert("a");
  if(d.get_dependents("x") == two) cout << "PASS" << endl;
  else cout << "a doesn't have the right dependents." << endl;

  std::cout << "Ended Test" << std::endl;
}
