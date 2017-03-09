Info
====

This is my solution to the challenge: http://followthewhiterabbit.trustpilot.com/

Usage info
==========

```
WhiteRabbit.exe < wordlist
```

Performance
===========

This solution is not optimized for multi-threading.

Nevertheless, the performance on Sandy Bridge @2.8GHz is as follows:

* If only phrases of at most 3 words are allowed, then it takes 2.5 seconds to find and check all anagrams; all relevant hashes are solved in first 0.4 seconds;

* If phrases of 4 words are allowed as well, then it takes 70 seconds to find and check all anagrams; all hashes are solved in first 5 seconds;

For comparison, certain other solutions available on GitHub seem to require 3 hours to find all 3-word anagrams (i.e. this solution is faster by a factor of 4000 in 3-word case).