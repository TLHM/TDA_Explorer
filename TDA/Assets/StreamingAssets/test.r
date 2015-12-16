theargs <- commandArgs()
library(dplyr)
data <- read.csv(theargs[4], header = FALSE)
filterType <- theargs[5]
data<-mutate(data, filter = V1 + V2 + V3)
data<-arrange(data, filter)
write.csv(data, theargs[6])
q()
