#include <bits/stdc++.h>

using namespace std;

int main() {
  double n, m, a;
  cin >> n >> m >> a;

  double x = ceil(n / a);
  double y = ceil(m / a);

  long long result = x*y;
  cout << result;
}