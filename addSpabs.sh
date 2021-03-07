#!/bin/bash

if ! type -a expand 1>null 2>1; then
	echo "'type -a expand failed!"
	echo Expand not found on system.
	exit 1
fi

if `git config --global -l | grep filter.spabs 1>null`; then
	echo "git config --global filter.spabs.* already set to something, not touching it!"
else
	echo "Spabs time ;)"
	git config --global filter.spabs.clean 'expand --initial -t 4'
	git config --global filter.spabs.smudge 'expand --initial -t 4'
fi
