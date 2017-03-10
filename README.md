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

This solution is partially optimized for multi-threading.

Single-threaded performance on Sandy Bridge @2.8GHz is as follows:

* If only phrases of at most 3 words are allowed, then it takes 2.5 seconds to find and check all anagrams; all relevant hashes are solved in first 0.4 seconds;

* If phrases of 4 words are allowed as well, then it takes 40 seconds to find and check all anagrams; all hashes are solved in first 3 seconds;

For comparison, certain other solutions available on GitHub seem to require 3 hours to find all 3-word anagrams (i.e. this solution is faster by a factor of 4000 in 3-word case).

Anagrams generation is not parallelized, as even single-threaded performance for 4-word anagrams is high enough; and 5-word (or larger) anagrams are frequent enough for most of the time being spent on computing hashes, with full CPU load.

Multi-threaded performance is as follows:

* If only phrases of at most 4 words are allowed, then it takes 20 seconds to find and check all anagrams; all hashes are solved in first 1.5 seconds

* If phrases of 5 words are allowed as well, then all hashes are solved in first 29 seconds. Around 50% of time is spent on MD5 computations for correct anagrams, so there is not a lot to optimize further.
