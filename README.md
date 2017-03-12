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

Memory usage is minimal (for that kind of task), around 10-30MB.

This solution is partially optimized for multi-threading.

It is also somewhat optimized for likely intended phrases, as anagrams consisting of longer words are generated first.
That's why the given hashes are solved much sooner than it takes to check all anagrams.

Single-threaded performance on Sandy Bridge @2.8GHz is as follows:

* If only phrases of at most 3 words are allowed, then it takes 2.5 seconds to find and check all 4560 anagrams; all relevant hashes are solved in first 0.4 seconds;

* If phrases of 4 words are allowed as well, then it takes 40 seconds to find and check all 7433016 anagrams; all hashes are solved in first 3 seconds;

For comparison, certain other solutions available on GitHub seem to require 3 hours to find all 3-word anagrams (i.e. this solution is faster by a factor of 4000 in 3-word case).

Anagrams generation is not parallelized, as even single-threaded performance for 4-word anagrams is high enough; and 5-word (or larger) anagrams are frequent enough for most of the time being spent on computing hashes, with full CPU load.

Multi-threaded performance with RyuJIT (.NET 4.6, 64-bit system) is as follows:

* If only phrases of at most 4 words are allowed, then it takes less than 10 seconds to find and check all 7433016 anagrams; all hashes are solved in first 1 second.

* If phrases of 5 words are allowed as well, then it takes around 18 minutes to find and check all anagrams; all hashes are solved in first 25 seconds. Most of time is spent on MD5 computations for correct anagrams, so there is not a lot to optimize further.

* If phrases of 6 words are allowed as well, then "more difficult" hash is solved in 50 seconds, "easiest" in 3.5 minutes, and "hard" in 6 minutes.

* If phrases of 7 words are allowed as well, then "more difficult" hash is solved in 6 minutes.

Note that all measurements were done on a Release build; Debug build is significantly slower.
