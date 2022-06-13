#include <bits/stdc++.h>
#include <vector>

using namespace std;

int main() {
  int n;
  int x = 0; // the result value
  string currentStatement;

  map<string, int> possibleOperations = {
      {"--X", -1},
      {"X--", -1},
      {"X++", 1},
      {"++X", 1},
  };

  cin >> n;

  vector<string> statements;

  for (int i = 0; i < n; i++) {
    cin >> currentStatement;
    statements.push_back(currentStatement);
  }

  for (int i = 0; i < statements.size(); i++) {
    int valueToAdd = possibleOperations[statements[i]];
    x += valueToAdd;
  }

  cout << x;
}