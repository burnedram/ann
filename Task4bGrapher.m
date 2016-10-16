function Task4bGrapher(file)
fileID = fopen(file);
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%d %f %f'; %neurons trainingError validationError
A = fscanf(fileID, spec, [3 Inf])';
fclose(fileID);

[path, name, ~] = fileparts(file);
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

x = A(:, 1);
ytraining = A(:, 2);
yvalidation = A(:, 3);

h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
plot(x, ytraining);
plot(x, yvalidation);
xlabel('Neurons in hidden layer');
ylabel('Classification_{error}');
legend({'Training', 'Validation'});
title('Back propagation');
saveas(h, outfile);

end