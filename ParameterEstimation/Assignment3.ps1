# Test case
.\ParameterEstimation.exe --inputfile=toydata.txt --mixtureCount=1

# Run and get various mixtures
if((Test-Path Output) -eq 0)
{
    New-Item Output -type directory
}

for($i=1; $i -le 5; $i++)
{
	$fileName = "Output\\" + $i+ ".txt"
	.\ParameterEstimation.exe --inputfile=toydata.txt --mixtureCount=$i > $fileName
}

# combine all outputs to single file
# gci *.txt -Recurse -File |sort -Property CreationTime |% {(gc $_) + "`n"} > all.txt


for($i=1; $i -le 5; $i++)
{
	$fileName = "Output\\" + $i+ ".txt"
	.\ParameterEstimation.exe --inputfile=hw3-finaldata.txt --mixtureCount=$i > $fileName
}