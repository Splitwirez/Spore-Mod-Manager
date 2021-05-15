#!/usr/bin/env python
# coding=utf-8

import threading
import webbrowser

import inkex
from inkex import Anchor

class ThreadWebsite(threading.Thread):
    """Visit the website without locking inkscape"""
    def __init__(self, url):
        threading.Thread.__init__(self)
        self.url = url

    def run(self):
        webbrowser.open(self.url)

class FollowLink(inkex.EffectExtension):
    """Get the first selected item and follow it's href/url"""
    def effect(self):
        for node in self.svg.selection.filter(Anchor):
            vwswli = ThreadWebsite(node.get('xlink:href'))
            vwswli.start()
            break

if __name__ == '__main__':
    FollowLink().run()
