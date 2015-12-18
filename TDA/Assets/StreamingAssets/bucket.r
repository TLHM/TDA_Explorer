theArgs <- commandArgs()
library(dplyr)
data <- read.csv(theArgs[4], header = TRUE)
range <- summarize(data, min=min(filter), max=max(filter))
fraction <- (range[1,"max"]-range[1,"min"])/as.numeric(theArgs[7])
index <- as.numeric(theArgs[6])
low <- range[1,"min"]+index*fraction-.05*fraction
high <- range[1,"min"]+(index+1)*fraction+.05*fraction
bucket <- filter(data, filter>=low, filter<high)
write.csv(bucket, theArgs[5])
q()
