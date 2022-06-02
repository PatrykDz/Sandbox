#include <bits/stdc++.h>

using namespace std;

int main() {
    int n;
    
    cin >> n;

    vector<string> words;
    string inputWord;
    string currentWord;

    for(int i = 0; i < n; i++) {
        cin >> inputWord;
        words.push_back(inputWord);
    }

    for(int i = 0; i < n; i++) {
        currentWord = words[i];
        if(currentWord.length() > 10){
            cout << currentWord[0] << currentWord.length() -2 << currentWord[currentWord.length() - 1] << "\n";
        } else {
            cout << currentWord << "\n";
        }
    }
}