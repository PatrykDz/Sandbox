#include <bits/stdc++.h>

using namespace std;

int main() {
  string s;
  cin >> s;

  char helloLetters[] = {'h', 'e', 'l', 'l', 'o'};
  int helloLettersLength = 5;

  int helloLetterIndex = 0;
  for (int i = 0; i < s.size(); i++) {
    if (s[i] == helloLetters[helloLetterIndex]) {
      helloLetterIndex++;
    }
  }

  if (helloLetterIndex == helloLettersLength) {
    cout << "YES";
    return 0;
  }

  cout << "NO";
}