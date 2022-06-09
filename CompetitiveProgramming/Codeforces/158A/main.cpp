#include <bits/stdc++.h>
 
using namespace std;
 
int main() {
    int n, k; // n - number of contestants, k-th place above which You advance to the next round
    
    int input;
    vector<int> scores;
 
    int numberOfPeopleAdvancing = 0;
 
    cin>>n>>k;
 
    // Load scores
    for(int i=0; i<n; i++)
    {
        cin>>input;
        scores.push_back(input);
    }
 
    int minimumNumberOfPointsToAdvance = scores[k-1];
 
    for(int i=0; i<scores.size();i++) {
        if(scores[i] >= minimumNumberOfPointsToAdvance && scores[i] > 0) {
            numberOfPeopleAdvancing++;
        }
    }
 
    cout<<numberOfPeopleAdvancing;
}