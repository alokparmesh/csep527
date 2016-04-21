# Test case
.\SequenceAligner.exe --sequences="deadly,ddgearlyk" --pValue --full

# Run and get score for all the various sequences
if((Test-Path Output) -eq 0)
{
    New-Item Output -type directory
}
$accessions = @("P15172","P17542","P10085","P16075","P13904","Q90477","Q8IU24","P22816","Q10574","O95363")

for($i=0; $i -lt $accessions.Length; $i++)
{
    for($j=$i + 1; $j -lt $accessions.Length; $j++)
    {
        $fileName = "Output\\" + $accessions[$i] + "-" + $accessions[$j] + ".txt"
        $accessionsParameter = $accessions[$i] + "," + $accessions[$j]
        .\SequenceAligner.exe --accessions="$accessionsParameter" --pValue > $fileName
    }
}

# combine all outputs to single file
# gci *.txt -Recurse -File |sort -Property CreationTime |% {(gc $_) + "`n"} > all.txt


# Test case
.\SequenceAligner.exe --sequences="deadly,ddgearlyk" --alignmentType=Global --pValue --full

for($i=0; $i -lt $accessions.Length; $i++)
{
    for($j=$i + 1; $j -lt $accessions.Length; $j++)
    {
        $fileName = "Output\\" + $accessions[$i] + "-" + $accessions[$j] + ".txt"
        $accessionsParameter = $accessions[$i] + "," + $accessions[$j]
        .\SequenceAligner.exe --accessions="$accessionsParameter" --alignmentType=Global --pValue > $fileName
    }
}