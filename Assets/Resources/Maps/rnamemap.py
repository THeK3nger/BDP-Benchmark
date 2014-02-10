import os
import os.path

for file in os.listdir("."):
	if file.endswith(".map"):
		os.rename(file,file+".txt")
