function outfile = Task3Grapher(fileTemplate, Ns)
file = sprintf(fileTemplate, 0);
[path, name, ~] = fileparts(file);
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end
h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
xlabel('\alpha');
ylabel('m_1');
title('Stochastic Hopfield');
legget = cell(length(Ns), 1);

for Ni = 1:length(Ns)
    N = Ns(Ni);
    file = sprintf(fileTemplate, N);
    fileID = fopen(file, 'r');
    if fileID == -1
        disp('Unable to open file');
        return
    end
    spec = '%f %f'; %alpha m_1
    A = fscanf(fileID, spec, [2 Inf])';
    fclose(fileID);

    plot(A(:, 1), A(:, 2));
    legget{Ni} = sprintf('N = %d', N);
end
legend(legget);
saveas(h, outfile);
end