function outfile = Task2Grapher(file)
fileID = fopen(file, 'r');
if fileID == -1
    disp('Unable to open file');
    return
end
spec = '%f %f %f %f %f %f %f'; %q 0 1 2 3 4 avg
A = fscanf(fileID, spec, [7 Inf])';
fclose(fileID);

[path, name, ~] = fileparts(file);
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
x = A(:, 1);
legget = cell(6, 1);
for i = 2:6
    y = A(:, i);
    plot(x, y);
    legget{i-1} = sprintf('Digit "%d"', i-2);
end
toty = A(:, 7);
plot(x, toty);
legget{6} = 'Average';

ylim([-0.025, 1.025]);
legend(legget, 'Location', 'Best');
xlabel('q');
ylabel('P_{correct}');
title('Distorted digit recognition');
saveas(h, outfile);

end