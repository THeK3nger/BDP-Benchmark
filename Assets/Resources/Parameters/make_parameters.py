DICT = {
'update_radius' : 1,
'seed':160,
'number_of_runs':500,
'scramble_rate':1,
'initial_randomness':0.20,
'scramble_amount':0.1,
'valid_revision_limit' : 0.1
}

def make_params_string(param_dict) :
	stringa = ""
	for key in param_dict :
		stringa += key + "=" + str(param_dict[key]) + '\n'
	return stringa

def printParam(namefile) :
	f = open(namefile,'w')
	f.write(make_params_string(DICT))
	f.close()

def printParamIteration(basename,key,range) :
	for i,k in enumerate(range) :
		DICT[key] = k
		printParam(basename+str(i)+".txt")
