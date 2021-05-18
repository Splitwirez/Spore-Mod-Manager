1. Follow PEP8
2. For Python 2 specific code use `if sys.version_info[0] == 2:` and include a Python3 alternative in the else clause.  


  For example:
  
```python
if sys.version_info[0] == 2:
    import urllib
    import urlparse
else:
    import urllib.request as urllib
    import urllib.parse as urlparse
```

