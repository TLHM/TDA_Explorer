theargs <- commandArgs()
theargs[3]
theargs[4]
library(dplyr)
data <- read.csv(theargs[4], header = FALSE)
filterType <- theargs[5]
data<-mutate(data, filter = V3)
data<-arrange(data, filter)
write.csv(data, theargs[6])
q()
